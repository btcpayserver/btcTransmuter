using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using BtcTransmuter.Abstractions.ExternalServices;
using BtcTransmuter.Abstractions.Recipes;
using BtcTransmuter.Abstractions.Triggers;
using BtcTransmuter.Data.Entities;
using BtcTransmuter.Data.Models;
using BtcTransmuter.Extension.Email.ExternalServices.Imap;
using BtcTransmuter.Extension.Email.ExternalServices.Pop3;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Caching.Memory;

namespace BtcTransmuter.Extension.Email.Triggers.ReceivedEmail
{
    [Authorize]
    [Route("email-plugin/triggers/[controller]")]
    public class ReceivedEmailController : BaseTriggerController<ReceivedEmailController.ReceivedEmailViewModel,
        ReceivedEmailTriggerParameters>
    {
        private readonly IExternalServiceManager _externalServiceManager;

        public ReceivedEmailController(IRecipeManager recipeManager, UserManager<User> userManager,
            IMemoryCache memoryCache, IExternalServiceManager externalServiceManager) : base(recipeManager, userManager,
            memoryCache, externalServiceManager)
        {
            _externalServiceManager = externalServiceManager;
        }

        protected override async Task<ReceivedEmailViewModel> BuildViewModel(RecipeTrigger data)
        {
            var services = await _externalServiceManager.GetExternalServicesData(new ExternalServicesDataQuery()
            {
                Type = new[] {Pop3Service.Pop3ExternalServiceType, ImapService.ImapExternalServiceType},
                UserId = GetUserId()
            });
            var innerData = data.Get<ReceivedEmailTriggerParameters>();

            return new ReceivedEmailViewModel()
            {
                ExternalServices = new SelectList(services, nameof(ExternalServiceData.Id),
                    nameof(ExternalServiceData.Name), data.ExternalServiceId),

                RecipeId = data.RecipeId,
                ExternalServiceId = data.ExternalServiceId,
                Body = innerData.Body,
                Subject = innerData.Subject,
                FromEmail = innerData.FromEmail,
                BodyComparer = innerData.BodyComparer,
                SubjectComparer = innerData.SubjectComparer
            };
        }

        protected override async Task<(RecipeTrigger ToSave, ReceivedEmailViewModel showViewModel)> BuildModel(
            ReceivedEmailViewModel viewModel, RecipeTrigger mainModel)
        {
            if (!string.IsNullOrEmpty(viewModel.Body) &&
                viewModel.BodyComparer == ReceivedEmailTriggerParameters.FieldComparer.None)
            {
                ModelState.AddModelError(nameof(ReceivedEmailViewModel.BodyComparer),
                    "Please choose a Body filter type");
            }

            if (!string.IsNullOrEmpty(viewModel.Subject) &&
                viewModel.SubjectComparer == ReceivedEmailTriggerParameters.FieldComparer.None)
            {
                ModelState.AddModelError(nameof(ReceivedEmailViewModel.SubjectComparer),
                    "Please choose a Subject filter type");
            }

            if (!ModelState.IsValid)
            {
                var pop3Services = await _externalServiceManager.GetExternalServicesData(new ExternalServicesDataQuery()
                {
                    Type = new[] {Pop3Service.Pop3ExternalServiceType, ImapService.ImapExternalServiceType},
                    UserId = GetUserId()
                });


                viewModel.ExternalServices = new SelectList(pop3Services, nameof(ExternalServiceData.Id),
                    nameof(ExternalServiceData.Name), viewModel.ExternalServiceId);
                return (null, viewModel);
            }

            var recipeTrigger = mainModel;
            recipeTrigger.ExternalServiceId = viewModel.ExternalServiceId;
            recipeTrigger.Set((ReceivedEmailTriggerParameters) viewModel);
            return (recipeTrigger, null);
        }

        public class ReceivedEmailViewModel : ReceivedEmailTriggerParameters
        {
            public string RecipeId { get; set; }
            public SelectList ExternalServices { get; set; }
            [Required] public string ExternalServiceId { get; set; }
        }
    }
}