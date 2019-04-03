using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using BtcTransmuter.Abstractions.Actions;
using BtcTransmuter.Abstractions.ExternalServices;
using BtcTransmuter.Abstractions.Recipes;
using BtcTransmuter.Data.Entities;
using BtcTransmuter.Data.Models;
using BtcTransmuter.Extension.Exchange.ExternalServices.Exchange;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Caching.Memory;

namespace BtcTransmuter.Extension.Exchange.Actions.GetExchangeRate
{
    [Route("exchange-plugin/actions/[controller]")]
    [Authorize]
    public class GetExchangeRateController : BaseActionController<GetExchangeRateController.GetExchangeRateViewModel, GetExchangeRateData>
    {
        private readonly IExternalServiceManager _externalServiceManager;

        public GetExchangeRateController(IMemoryCache memoryCache,
            UserManager<User> userManager,
            IRecipeManager recipeManager, IExternalServiceManager externalServiceManager) : base(memoryCache,
            userManager, recipeManager, externalServiceManager)
        {
            _externalServiceManager = externalServiceManager;
        }

        protected override async Task<GetExchangeRateViewModel> BuildViewModel(RecipeAction from)
        {
            var services = await _externalServiceManager.GetExternalServicesData(new ExternalServicesDataQuery()
            {
                Type = new[] {ExchangeService.ExchangeServiceType},
                UserId = GetUserId()
            });
            var fromData = from.Get<GetExchangeRateData>();
            return new GetExchangeRateViewModel
            {
                RecipeId = @from.RecipeId,
                ExternalServiceId = @from.ExternalServiceId,
                MarketSymbol = fromData.MarketSymbol,
                ExternalServices = new SelectList(services, nameof(ExternalServiceData.Id),
                    nameof(ExternalServiceData.Name), @from.ExternalServiceId)
            };
        }

        protected override async Task<(RecipeAction ToSave, GetExchangeRateViewModel showViewModel)> BuildModel(
            GetExchangeRateViewModel viewModel, RecipeAction mainModel)
        {
            if (ModelState.IsValid)
            {
                var serviceData =
                    await _externalServiceManager.GetExternalServiceData(viewModel.ExternalServiceId, GetUserId());
                var exchangeService = new ExchangeService(serviceData);
                var symbols = (await exchangeService.ConstructClient().GetMarketSymbolsAsync()).ToArray();
                if (symbols.Contains(viewModel.MarketSymbol))
                {
                    mainModel.ExternalServiceId = viewModel.ExternalServiceId;
                    mainModel.Set<GetExchangeRateData>(viewModel);
                    return (mainModel, null);
                }

                ModelState.AddModelError(nameof(GetExchangeRateViewModel.MarketSymbol),
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

        public class GetExchangeRateViewModel : GetExchangeRateData, IUseExternalService, IActionViewModel
        {
            public string RecipeId { get; set; }
            public string RecipeActionIdInGroupBeforeThisOne { get; set; }
            public SelectList ExternalServices { get; set; }
            [Required][Display(Name = "Exchange Service")] public string ExternalServiceId { get; set; }
        }
    }
}