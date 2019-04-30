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
        ReceivedEmailTriggerParameters>
    {
        public override string TriggerId => new ReceivedEmailTrigger().Id;
        public override string Name => "Receive Email";

        public  override string Description =>
            "Trigger a recipe by receiving a specifically formatted email through a pop3 or imap  external service.";

        public override  string ViewPartial => "ViewReceivedEmailTrigger";
        public override string ControllerName => "ReceivedEmail";

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
                    if (!triggerData.Subject.Equals(parameters.Subject))
                    {
                        return Task.FromResult(false);
                    }

                    break;
                case ReceivedEmailTriggerParameters.FieldComparer.Contains:
                    if (!triggerData.Subject.Contains(parameters.Subject))
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
                    if (!triggerData.Body.Contains(parameters.Body))
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