using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using BtcTransmuter.Abstractions.Actions;
using BtcTransmuter.Abstractions.ExternalServices;
using BtcTransmuter.Abstractions.Recipes;
using BtcTransmuter.Data.Entities;
using BtcTransmuter.Data.Models;
using BtcTransmuter.Extension.Lightning.ExternalServices.NBXplorerWallet;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Caching.Memory;

namespace BtcTransmuter.Extension.NBXplorer.Actions.NBXplorerSignPSBT
{
    [Route("nbxplorer-plugin/actions/sign-psbt")]
    [Authorize]
    public class NBXplorerSignPSBTController : BaseActionController<
        NBXplorerSignPSBTController.NBXplorerSignPSBTViewModel,
        NBXplorerSignPSBTData>
    {
        private readonly IExternalServiceManager _externalServiceManager;

        public NBXplorerSignPSBTController(IMemoryCache memoryCache,
            UserManager<User> userManager,
            IRecipeManager recipeManager,
            IExternalServiceManager externalServiceManager) : base(
            memoryCache,
            userManager, recipeManager)
        {
            _externalServiceManager = externalServiceManager;
        }

        protected override async Task<NBXplorerSignPSBTViewModel> BuildViewModel(RecipeAction from)
        {
            var services = await _externalServiceManager.GetExternalServicesData(new ExternalServicesDataQuery()
            {
                Type = new[] {NBXplorerWalletService.NBXplorerWalletServiceType},
                UserId = GetUserId()
            });
            var fromData = from.Get<NBXplorerSignPSBTData>();
            return new NBXplorerSignPSBTViewModel
            {
                RecipeId = from.RecipeId,
                PSBT = fromData.PSBT,
                ExternalServiceId = from.ExternalServiceId,
                ExternalServices = new SelectList(services, nameof(ExternalServiceData.Id),
                    nameof(ExternalServiceData.Name), from.ExternalServiceId)
            };
        }

        protected override async Task<(RecipeAction ToSave, NBXplorerSignPSBTViewModel showViewModel)> BuildModel(
            NBXplorerSignPSBTViewModel viewModel, RecipeAction mainModel)
        {
            if (ModelState.IsValid)
            {
                mainModel.ExternalServiceId = viewModel.ExternalServiceId;
                mainModel.Set<NBXplorerSignPSBTData>(viewModel);
                return (mainModel, null);
            }

            var services = await _externalServiceManager.GetExternalServicesData(new ExternalServicesDataQuery()
            {
                Type = new[] {NBXplorerWalletService.NBXplorerWalletServiceType},
                UserId = GetUserId()
            });
            viewModel.ExternalServices = new SelectList(services, nameof(ExternalServiceData.Id),
                nameof(ExternalServiceData.Name), viewModel.ExternalServiceId);
            return (null, viewModel);
        }

        public class NBXplorerSignPSBTViewModel : NBXplorerSignPSBTData, IActionViewModel, IUseExternalService
        {
            public string RecipeId { get; set; }
            public string RecipeActionIdInGroupBeforeThisOne { get; set; }
            public SelectList ExternalServices { get; set; }

            [Display(Name = "NBXplorer Wallet External Service")]
            [Required]
            public string ExternalServiceId { get; set; }
        }
    }
}