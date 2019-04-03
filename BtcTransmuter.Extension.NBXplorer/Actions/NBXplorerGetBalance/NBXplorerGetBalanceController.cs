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

namespace BtcTransmuter.Extension.NBXplorer.Actions.NBXplorerGetBalance
{
    [Route("nbxplorer-plugin/actions/[controller]")]
    [Authorize]
    public class NBXplorerGetBalanceController : BaseActionController<
        NBXplorerGetBalanceController.NBXplorerGetBalanceViewModel,
        NBXplorerGetBalanceData>
    {
        private readonly IExternalServiceManager _externalServiceManager;

        public NBXplorerGetBalanceController(IMemoryCache memoryCache,
            UserManager<User> userManager,
            IRecipeManager recipeManager,
            IExternalServiceManager externalServiceManager) : base(
            memoryCache,
            userManager, recipeManager, externalServiceManager)
        {
            _externalServiceManager = externalServiceManager;
        }

        protected override async Task<NBXplorerGetBalanceViewModel> BuildViewModel(RecipeAction from)
        {
            var services = await _externalServiceManager.GetExternalServicesData(new ExternalServicesDataQuery()
            {
                Type = new[] {NBXplorerWalletService.NBXplorerWalletServiceType},
                UserId = GetUserId()
            });
            return new NBXplorerGetBalanceViewModel
            {
                RecipeId = from.RecipeId,
                ExternalServiceId = from.ExternalServiceId,
                ExternalServices = new SelectList(services, nameof(ExternalServiceData.Id),
                    nameof(ExternalServiceData.Name), from.ExternalServiceId)
            };
        }

        protected override async Task<(RecipeAction ToSave, NBXplorerGetBalanceViewModel showViewModel)> BuildModel(
            NBXplorerGetBalanceViewModel viewModel, RecipeAction mainModel)
        {
            if (ModelState.IsValid)
            {
                mainModel.ExternalServiceId = viewModel.ExternalServiceId;
                mainModel.Set<NBXplorerGetBalanceData>(viewModel);
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

        public class NBXplorerGetBalanceViewModel : NBXplorerGetBalanceData, IActionViewModel, IUseExternalService
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