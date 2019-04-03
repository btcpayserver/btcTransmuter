using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using BtcTransmuter.Abstractions.ExternalServices;
using BtcTransmuter.Abstractions.Recipes;
using BtcTransmuter.Abstractions.Triggers;
using BtcTransmuter.Data.Entities;
using BtcTransmuter.Data.Models;
using BtcTransmuter.Extension.Lightning.ExternalServices.NBXplorerWallet;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Caching.Memory;
using NBitcoin;

namespace BtcTransmuter.Extension.NBXplorer.Triggers.NBXplorerBalance
{
    [Authorize]
    [Route("nbxplorer-plugin/triggers/[controller]")]
    public class NBXplorerBalanceController : BaseTriggerController<
        NBXplorerBalanceController.NBXplorerBalanceViewModel,
        NBXplorerBalanceTriggerParameters>
    {
        private readonly IExternalServiceManager _externalServiceManager;

        public NBXplorerBalanceController(IRecipeManager recipeManager, UserManager<User> userManager,
            IMemoryCache memoryCache,
            IExternalServiceManager externalServiceManager) : base(recipeManager, userManager,
            memoryCache, externalServiceManager)
        {
            _externalServiceManager = externalServiceManager;
        }

        protected override async Task<NBXplorerBalanceViewModel> BuildViewModel(RecipeTrigger data)
        {
            var innerData = data.Get<NBXplorerBalanceTriggerParameters>();
            var services = await _externalServiceManager.GetExternalServicesData(new ExternalServicesDataQuery()
            {
                Type = new[] {NBXplorerWalletService.NBXplorerWalletServiceType},
                UserId = GetUserId()
            });
            return new NBXplorerBalanceViewModel()
            {
                RecipeId = data.RecipeId,
                ExternalServiceId = data.ExternalServiceId,
                ExternalServices = new SelectList(services, nameof(ExternalServiceData.Id),
                    nameof(ExternalServiceData.Name), data.ExternalServiceId),
                BalanceComparer = innerData.BalanceComparer,
                BalanceValue = innerData.Balance?.ToUnit(MoneyUnit.BTC) ?? 0,
                BalanceMoneyUnit = MoneyUnit.BTC
            };
        }

        protected override async Task<(RecipeTrigger ToSave, NBXplorerBalanceViewModel showViewModel)>
            BuildModel(
                NBXplorerBalanceViewModel viewModel, RecipeTrigger mainModel)
        {
            if (!ModelState.IsValid)
            {
                var services = await _externalServiceManager.GetExternalServicesData(new ExternalServicesDataQuery()
                {
                    Type = new[] {NBXplorerWalletService.NBXplorerWalletServiceType},
                    UserId = GetUserId()
                });
                viewModel.ExternalServices = new SelectList(services, nameof(ExternalServiceData.Id),
                    nameof(ExternalServiceData.Name), viewModel.ExternalServiceId);

                return (null, viewModel);
            }

            var recipeTrigger = mainModel;

            recipeTrigger.ExternalServiceId = viewModel.ExternalServiceId;
            recipeTrigger.Set((NBXplorerBalanceTriggerParameters) viewModel);

            return (recipeTrigger, null);
        }

        public class NBXplorerBalanceViewModel : NBXplorerBalanceTriggerParameters, IUseExternalService
        {
            public string RecipeId { get; set; }
            public SelectList ExternalServices { get; set; }

            [Display(Name = "NBXplorer Wallet External Service")]
            [Required]
            public string ExternalServiceId { get; set; }
        }
    }
}