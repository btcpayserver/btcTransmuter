using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using BtcTransmuter.Abstractions.Actions;
using BtcTransmuter.Abstractions.ExternalServices;
using BtcTransmuter.Abstractions.Recipes;
using BtcTransmuter.Data.Entities;
using BtcTransmuter.Data.Models;
using BtcTransmuter.Extension.Exchange.ExternalServices.Exchange;
using ExchangeSharp;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Caching.Memory;

namespace BtcTransmuter.Extension.Exchange.Actions.PlaceOrder
{
    [Route("exchange-plugin/actions/[controller]")]
    [Authorize]
    public class PlaceOrderController : BaseActionController<PlaceOrderController.PlaceOrderViewModel, PlaceOrderData>
    {
        private readonly IExternalServiceManager _externalServiceManager;

        public PlaceOrderController(IMemoryCache memoryCache,
            UserManager<User> userManager,
            IRecipeManager recipeManager, IExternalServiceManager externalServiceManager) : base(memoryCache,
            userManager, recipeManager, externalServiceManager)
        {
            _externalServiceManager = externalServiceManager;
        }

        [HttpGet("symbols/{externalServiceId}")]
        public async Task<string[]> GetAvailableMarketSymbols(string externalServiceId)
        {
            var serviceData =
                await _externalServiceManager.GetExternalServiceData(externalServiceId, GetUserId());
            var exchangeService = new ExchangeService(serviceData);
            return (await exchangeService.ConstructClient().GetMarketSymbolsAsync()).ToArray();
        }

        protected override async Task<PlaceOrderViewModel> BuildViewModel(RecipeAction from)
        {
            var services = await _externalServiceManager.GetExternalServicesData(new ExternalServicesDataQuery()
            {
                Type = new[] {ExchangeService.ExchangeServiceType},
                UserId = GetUserId()
            });
            var fromData = from.Get<PlaceOrderData>();
            return new PlaceOrderViewModel
            {
                RecipeId = @from.RecipeId,
                ExternalServiceId = @from.ExternalServiceId,
                Price = fromData.Price,
                Amount = fromData.Amount,
                IsBuy = fromData.IsBuy,
                IsMargin = fromData.IsMargin,
                OrderType = fromData.OrderType,
                StopPrice = fromData.StopPrice,
                MarketSymbol = fromData.MarketSymbol,
                ExternalServices = new SelectList(services, nameof(ExternalServiceData.Id),
                    nameof(ExternalServiceData.Name), @from.ExternalServiceId)
            };
        }

        protected override async Task<(RecipeAction ToSave, PlaceOrderViewModel showViewModel)> BuildModel(
            PlaceOrderViewModel viewModel, RecipeAction mainModel)
        {
            if (viewModel.OrderType == OrderType.Stop && string.IsNullOrEmpty(viewModel.StopPrice))
            {
                ModelState.AddModelError(nameof(PlaceOrderViewModel.StopPrice),
                    $"Please set a stop price if you wish to place a stop order");
            }
            
            if (ModelState.IsValid)
            {
                var serviceData =
                    await _externalServiceManager.GetExternalServiceData(viewModel.ExternalServiceId, GetUserId());
                var exchangeService = new ExchangeService(serviceData);
                var symbols = (await exchangeService.ConstructClient().GetMarketSymbolsAsync()).ToArray();
                if (symbols.Contains(viewModel.MarketSymbol))
                {
                    mainModel.ExternalServiceId = viewModel.ExternalServiceId;
                    mainModel.Set<PlaceOrderData>(viewModel);
                    return (mainModel, null);
                }

                ModelState.AddModelError(nameof(PlaceOrderViewModel.MarketSymbol),
                    $"The market symbols you entered is invalid. Please choose from the following: {string.Join(",", symbols)}");
            }

            var services = await _externalServiceManager.GetExternalServicesData(new ExternalServicesDataQuery()
            {
                Type = new[] {ExchangeService.ExchangeServiceType},
                UserId = GetUserId()
            });

            viewModel.ExternalServices = new SelectList(services, nameof(ExternalServiceData.Id),
                nameof(ExternalServiceData.Name), viewModel.ExternalServiceId);
            return (null, viewModel);
        }

        public class PlaceOrderViewModel : PlaceOrderData, IUseExternalService, IActionViewModel
        {
            public string RecipeId { get; set; }
            public string RecipeActionIdInGroupBeforeThisOne { get; set; }
            public SelectList ExternalServices { get; set; }
            [Required][Display(Name = "Exchange Service")] public string ExternalServiceId { get; set; }
        }
    }
}