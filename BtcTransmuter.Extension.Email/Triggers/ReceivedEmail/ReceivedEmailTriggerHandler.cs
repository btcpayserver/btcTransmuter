using System;
using System.Threading.Tasks;
using BtcTransmuter.Abstractions;
using BtcTransmuter.Abstractions.Triggers;
using BtcTransmuter.Data;

namespace BtcTransmuter.Extension.Email.Triggers
{
    public class ReceivedEmailTriggerHandler : BaseTriggerHandler<ReceivedEmailTriggerData,
        ReceivedEmailTriggerParameters>
    {
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