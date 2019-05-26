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

namespace BtcTransmuter.Extension.Exchange.Actions.GetExchangeBalance
{
    [Route("exchange-plugin/actions/[controller]")]
    [Authorize]
    public class GetExchangeBalanceController : BaseActionController<GetExchangeBalanceController.GetExchangeBalanceViewModel, GetExchangeBalanceData>
    {
        private readonly IExternalServiceManager _externalServiceManager;

        public GetExchangeBalanceController(IMemoryCache memoryCache,
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
            var symbols = await exchangeService.ConstructClient().GetCurrenciesAsync();

            return symbols.Keys.ToArray();
        }

        protected override async Task<GetExchangeBalanceViewModel> BuildViewModel(RecipeAction from)
        {
            var services = await _externalServiceManager.GetExternalServicesData(new ExternalServicesDataQuery()
            {
                Type = new[] {ExchangeService.ExchangeServiceType},
                UserId = GetUserId()
            });
            var fromData = from.Get<GetExchangeBalanceData>();
            return new GetExchangeBalanceViewModel
            {
                RecipeId = @from.RecipeId,
                ExternalServiceId = @from.ExternalServiceId,
                Asset = fromData.Asset,
                ExternalServices = new SelectList(services, nameof(ExternalServiceData.Id),
                    nameof(ExternalServiceData.Name), @from.ExternalServiceId)
            };
        }

        protected override async Task<(RecipeAction ToSave, GetExchangeBalanceViewModel showViewModel)> BuildModel(
            GetExchangeBalanceViewModel viewModel, RecipeAction mainModel)
        {
            
            if (ModelState.IsValid)
            {
                mainModel.ExternalServiceId = viewModel.ExternalServiceId;
                mainModel.Set<GetExchangeBalanceData>(viewModel);
                return (mainModel, null);
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

        public class GetExchangeBalanceViewModel : GetExchangeBalanceData, IUseExternalService, IActionViewModel
        {
            public string RecipeId { get; set; }
            public string RecipeActionIdInGroupBeforeThisOne { get; set; }
            public SelectList ExternalServices { get; set; }
            [Required][Display(Name = "Exchange Service")] public string ExternalServiceId { get; set; }
        }
    }
}