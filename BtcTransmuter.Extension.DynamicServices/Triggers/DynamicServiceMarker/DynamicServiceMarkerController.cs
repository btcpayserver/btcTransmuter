using System.Threading.Tasks;
using BtcTransmuter.Abstractions.ExternalServices;
using BtcTransmuter.Abstractions.Recipes;
using BtcTransmuter.Abstractions.Triggers;
using BtcTransmuter.Data.Entities;
using BtcTransmuter.Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace BtcTransmuter.Extension.Webhook.Triggers.DynamicServiceMarker
{
    [Authorize]
    [Route("DynamicService-plugin/triggers/[controller]")]
    public class DynamicServiceMarkerController : BaseTriggerController<DynamicServiceMarkerTriggerParametersViewModel, DynamicServiceMarkerTriggerParameters>
    {

        public DynamicServiceMarkerController(IRecipeManager recipeManager, UserManager<User> userManager,
            IMemoryCache memoryCache,
            IExternalServiceManager externalServiceManager) : base(recipeManager, userManager, memoryCache,
            externalServiceManager)
        {
        }

        protected override Task<DynamicServiceMarkerTriggerParametersViewModel> BuildViewModel(RecipeTrigger data)
        {
            return Task.FromResult(new DynamicServiceMarkerTriggerParametersViewModel()
            {
                RecipeId = data.RecipeId
            });
        }

        protected override Task<(RecipeTrigger ToSave, DynamicServiceMarkerTriggerParametersViewModel showViewModel)> BuildModel(
            DynamicServiceMarkerTriggerParametersViewModel viewModel, RecipeTrigger mainModel)
        {
           mainModel.Set(viewModel);

            return Task.FromResult<(RecipeTrigger ToSave, DynamicServiceMarkerTriggerParametersViewModel showViewModel)>((mainModel,
                null));
        }

      
    }
}