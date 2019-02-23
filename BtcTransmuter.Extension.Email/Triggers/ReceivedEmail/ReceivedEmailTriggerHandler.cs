using System;
using System.Threading.Tasks;
using BtcTransmuter.Abstractions.Helpers;
using BtcTransmuter.Abstractions.Triggers;
using BtcTransmuter.Data;
using BtcTransmuter.Data.Entities;
using BtcTransmuter.Extension.Email.ExternalServices;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;

namespace BtcTransmuter.Extension.Email.Triggers.ReceivedEmail
{
    public class ReceivedEmailTriggerHandler : BaseTriggerHandler<ReceivedEmailTriggerData,
        ReceivedEmailTriggerParameters>, ITriggerDescriptor
    {
        public override string TriggerId => new ReceivedEmailTrigger().Id;
        public string Name => "Receive Email";

        public string Description =>
            "Trigger a recipe by receiving a specifically formatted email through a pop3 or imap  external service.";

        public string ViewPartial => "ViewReceivedEmailTrigger";
        public Task<IActionResult> EditData(RecipeTrigger data)
        {
            using (var scope = DependencyHelper.ServiceScopeFactory.CreateScope())
            {
                var identifier = $"{Guid.NewGuid()}";
                var memoryCache = scope.ServiceProvider.GetService<IMemoryCache>();
                memoryCache.Set(identifier, data, new MemoryCacheEntryOptions()
                {
                    SlidingExpiration = TimeSpan.FromMinutes(60)
                });

                return Task.FromResult<IActionResult>(new RedirectToActionResult(
                    nameof(ReceivedEmailController.EditData),
                    "ReceivedEmail", new
                    {
                        identifier
                    }));
            }
        }


        protected override Task<bool> IsTriggered(ITrigger trigger, RecipeTrigger recipeTrigger,
            ReceivedEmailTriggerData triggerData,
            ReceivedEmailTriggerParameters parameters)
        {
            if (!string.IsNullOrEmpty(parameters.FromEmail) && parameters.FromEmail.Equals(triggerData.FromEmail,
                    StringComparison.InvariantCultureIgnoreCase))
            {
                return Task.FromResult(false);
            }

            switch (parameters.SubjectComparer)
            {
                case ReceivedEmailTriggerParameters.FieldComparer.None:
                    break;
                case ReceivedEmailTriggerParameters.FieldComparer.Equals:
                    if (!triggerData.Subject.Equals(parameters.Subject, StringComparison.CurrentCultureIgnoreCase))
                    {
                        return Task.FromResult(false);
                    }

                    break;
                case ReceivedEmailTriggerParameters.FieldComparer.Contains:
                    if (!triggerData.Subject.Contains(parameters.Subject, StringComparison.CurrentCultureIgnoreCase))
                    {
                        return Task.FromResult(false);
                    }

                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            switch (parameters.BodyComparer)
            {
                case ReceivedEmailTriggerParameters.FieldComparer.None:
                    break;
                case ReceivedEmailTriggerParameters.FieldComparer.Equals:
                    if (!triggerData.Body.Equals(parameters.Body, StringComparison.CurrentCultureIgnoreCase))
                    {
                        return Task.FromResult(false);
                    }

                    break;
                case ReceivedEmailTriggerParameters.FieldComparer.Contains:
                    if (!triggerData.Body.Contains(parameters.Body, StringComparison.CurrentCultureIgnoreCase))
                    {
                        return Task.FromResult(false);
                    }

                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return Task.FromResult(true);
        }
    }
}