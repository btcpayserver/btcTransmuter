using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using BtcTransmuter.Abstractions.ExternalServices;
using BtcTransmuter.Abstractions.Recipes;
using BtcTransmuter.Abstractions.Triggers;
using BtcTransmuter.Data.Entities;
using BtcTransmuter.Data.Models;
using BtcTransmuter.Extension.Exchange.ExternalServices.Exchange;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Caching.Memory;

namespace BtcTransmuter.Extension.Exchange.Triggers.CheckExchangeBalance
{
    [Authorize]
    [Route("exchange-plugin/triggers/[controller]")]
    public class CheckExchangeBalanceController : BaseTriggerController<
        CheckExchangeBalanceController.CheckExchangeBalanceViewModel,
        CheckExchangeBalanceTriggerParameters>
    {
        private readonly IExternalServiceManager _externalServiceManager;

        public CheckExchangeBalanceController(IRecipeManager recipeManager, UserManager<User> userManager,
            IMemoryCache memoryCache,
            IExternalServiceManager externalServiceManager) : base(recipeManager, userManager,
            memoryCache, externalServiceManager)
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


        protected override async Task<CheckExchangeBalanceViewModel> BuildViewModel(RecipeTrigger data)
        {
            var innerData = data.Get<CheckExchangeBalanceTriggerParameters>();
            var services = await _externalServiceManager.GetExternalServicesData(new ExternalServicesDataQuery()
            {
                Type = new[] {ExchangeService.ExchangeServiceType},
                UserId = GetUserId()
            });
            return new CheckExchangeBalanceViewModel()
            {
                RecipeId = data.RecipeId,
                ExternalServiceId = data.ExternalServiceId,
                ExternalServices = new SelectList(services, nameof(ExternalServiceData.Id),
                    nameof(ExternalServiceData.Name), data.ExternalServiceId),
                BalanceValue = innerData.BalanceValue,
                BalanceComparer = innerData.BalanceComparer,
                Asset = innerData.Asset
            };
        }

        protected override async Task<(RecipeTrigger ToSave, CheckExchangeBalanceViewModel showViewModel)>
            BuildModel(
                CheckExchangeBalanceViewModel viewModel, RecipeTrigger mainModel)
        {
            if (!ModelState.IsValid)
            {
                var services = await _externalServiceManager.GetExternalServicesData(new ExternalServicesDataQuery()
                {
                    Type = new[] {ExchangeService.ExchangeServiceType},
                    UserId = GetUserId()
                });
                viewModel.ExternalServices = new SelectList(services, nameof(ExternalServiceData.Id),
                    nameof(ExternalServiceData.Name), viewModel.ExternalServiceId);

                return (null, viewModel);
            }

            var recipeTrigger = mainModel;

            recipeTrigger.ExternalServiceId = viewModel.ExternalServiceId;
            recipeTrigger.Set((CheckExchangeBalanceTriggerParameters) viewModel);

            return (recipeTrigger, null);
        }

        public class CheckExchangeBalanceViewModel : CheckExchangeBalanceTriggerParameters, IUseExternalService
        {
            public string RecipeId { get; set; }
            public SelectList ExternalServices { get; set; }

            [Display(Name = "Exchange External Service")]
            [Required]
            public string ExternalServiceId { get; set; }
        }
    }
}