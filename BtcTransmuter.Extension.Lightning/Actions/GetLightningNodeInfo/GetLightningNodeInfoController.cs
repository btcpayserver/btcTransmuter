using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using BtcTransmuter.Abstractions.Actions;
using BtcTransmuter.Abstractions.ExternalServices;
using BtcTransmuter.Abstractions.Recipes;
using BtcTransmuter.Data.Entities;
using BtcTransmuter.Data.Models;
using BtcTransmuter.Extension.Lightning.ExternalServices.LightningNode;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Caching.Memory;

namespace BtcTransmuter.Extension.Lightning.Actions.GetLightningNodeInfo
{
    [Route("lightning-plugin/actions/[controller]")]
    [Authorize]
    public class GetLightningNodeInfoController : BaseActionController<GetLightningNodeInfoController.GetLightningNodeInfoViewModel, GetLightningNodeInfoData>
    {
        private readonly IExternalServiceManager _externalServiceManager;

        public GetLightningNodeInfoController(IMemoryCache memoryCache, UserManager<User> userManager,
            IRecipeManager recipeManager, IExternalServiceManager externalServiceManager) : base(memoryCache,
            userManager, recipeManager,externalServiceManager)
        {
            _externalServiceManager = externalServiceManager;
        }

        protected override async Task<GetLightningNodeInfoViewModel> BuildViewModel(RecipeAction from)
        {
            var fromData = from.Get<GetLightningNodeInfoData>();
            var services = await _externalServiceManager.GetExternalServicesData(new ExternalServicesDataQuery()
            {
                Type = new[] {LightningNodeService.LightningNodeServiceType},
                UserId = GetUserId()
            });
            return new GetLightningNodeInfoViewModel
            {
                
                RecipeId = from.RecipeId,
                ExternalServiceId = from.ExternalServiceId,
                ExternalServices = new SelectList(services, nameof(ExternalServiceData.Id),
                    nameof(ExternalServiceData.Name), from.ExternalServiceId),
            };
        }

        protected override async Task<(RecipeAction ToSave, GetLightningNodeInfoViewModel showViewModel)> BuildModel(
            GetLightningNodeInfoViewModel viewModel, RecipeAction mainModel)
        {
            if (ModelState.IsValid)
            {
                mainModel.ExternalServiceId = viewModel.ExternalServiceId;
                mainModel.Set<GetLightningNodeInfoData>(viewModel);
                return (mainModel, null);
            }

            var services = await _externalServiceManager.GetExternalServicesData(new ExternalServicesDataQuery()
            {
                Type = new[] {LightningNodeService.LightningNodeServiceType},
                UserId = GetUserId()
            });


            viewModel.ExternalServices = new SelectList(services, nameof(ExternalServiceData.Id),
                nameof(ExternalServiceData.Name), viewModel.ExternalServiceId);
            return (null, viewModel);
        }

        public class GetLightningNodeInfoViewModel : GetLightningNodeInfoData, IUseExternalService, IActionViewModel
        {
            public string RecipeId { get; set; }
            public string RecipeActionIdInGroupBeforeThisOne { get; set; }
            public SelectList ExternalServices { get; set; }
            [Display(Name = "Lightning Node External Service")]
            [Required] public string ExternalServiceId { get; set; }
        }
    }
}