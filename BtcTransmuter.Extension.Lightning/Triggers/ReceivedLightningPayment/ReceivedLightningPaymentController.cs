using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using BtcTransmuter.Abstractions.ExternalServices;
using BtcTransmuter.Abstractions.Recipes;
using BtcTransmuter.Abstractions.Triggers;
using BtcTransmuter.Data.Entities;
using BtcTransmuter.Data.Models;
using BtcTransmuter.Extension.Lightning.ExternalServices.LightningNode;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Caching.Memory;

namespace BtcTransmuter.Extension.Lightning.Triggers.ReceivedLightningPayment
{
    [Authorize]
    [Route("email-plugin/triggers/[controller]")]
    public class ReceivedLightningPaymentController : BaseTriggerController<ReceivedLightningPaymentController.ReceivedLightningPaymentViewModel,
        ReceivedLightningPaymentTriggerParameters>
    {
        private readonly IExternalServiceManager _externalServiceManager;

        public ReceivedLightningPaymentController(IRecipeManager recipeManager, UserManager<User> userManager,
            IMemoryCache memoryCache, IExternalServiceManager externalServiceManager) : base(recipeManager, userManager,
            memoryCache, externalServiceManager)
        {
            _externalServiceManager = externalServiceManager;
        }

        protected override async Task<ReceivedLightningPaymentViewModel> BuildViewModel(RecipeTrigger data)
        {
            var services = await _externalServiceManager.GetExternalServicesData(new ExternalServicesDataQuery()
            {
                Type = new[] {LightningNodeService.LightningNodeServiceType},
                UserId = GetUserId()
            });
            var innerData = data.Get<ReceivedLightningPaymentTriggerParameters>();

            return new ReceivedLightningPaymentViewModel()
            {
                ExternalServices = new SelectList(services, nameof(ExternalServiceData.Id),
                    nameof(ExternalServiceData.Name), data.ExternalServiceId),

                RecipeId = data.RecipeId,
                ExternalServiceId = data.ExternalServiceId
            };
        }

        protected override async Task<(RecipeTrigger ToSave, ReceivedLightningPaymentViewModel showViewModel)> BuildModel(
            ReceivedLightningPaymentViewModel viewModel, RecipeTrigger mainModel)
        {
            
            if (!ModelState.IsValid)
            {
                var pop3Services = await _externalServiceManager.GetExternalServicesData(new ExternalServicesDataQuery()
                {
                    Type = new[] {LightningNodeService.LightningNodeServiceType},
                    UserId = GetUserId()
                });


                viewModel.ExternalServices = new SelectList(pop3Services, nameof(ExternalServiceData.Id),
                    nameof(ExternalServiceData.Name), viewModel.ExternalServiceId);
                return (null, viewModel);
            }

            var recipeTrigger = mainModel;
            recipeTrigger.ExternalServiceId = viewModel.ExternalServiceId;
            recipeTrigger.Set((ReceivedLightningPaymentTriggerParameters) viewModel);
            return (recipeTrigger, null);
        }

        public class ReceivedLightningPaymentViewModel : ReceivedLightningPaymentTriggerParameters
        {
            public string RecipeId { get; set; }
            public SelectList ExternalServices { get; set; }
            [Required] public string ExternalServiceId { get; set; }
        }
    }
}