using System;
using System.Threading.Tasks;
using BtcTransmuter.Abstractions;

namespace BtcTransmuter.Extension.Email.Triggers
{
    public class ReceivedEmailTriggerHandler : BaseTriggerHandler<ReceivedEmailTrigger, ReceivedEmailTriggerData,
        ReceivedEmailTriggerParameters>
    {
        protected override Task<bool> IsTriggered(ITrigger<ReceivedEmailTriggerData> trigger,
            ReceivedEmailTriggerParameters parameters)
        {
            if (trigger.Data.ExternalServiceId != parameters.ExternalServiceId)
            {
                return Task.FromResult(false);
            }

            if (!string.IsNullOrEmpty(parameters.FromEmail) && parameters.FromEmail.Equals(trigger.Data.FromEmail,
                    StringComparison.InvariantCultureIgnoreCase))
            {
                return Task.FromResult(false);
            }

            switch (parameters.SubjectComparer)
            {
                case ReceivedEmailTriggerParameters.FieldComparer.None:
                    break;
                case ReceivedEmailTriggerParameters.FieldComparer.Equals:
                    if (!trigger.Data.Subject.Equals(parameters.Subject, StringComparison.CurrentCultureIgnoreCase))
                    {
                        return Task.FromResult(false);
                    }

                    break;
                case ReceivedEmailTriggerParameters.FieldComparer.Contains:
                    if (!trigger.Data.Subject.Contains(parameters.Subject, StringComparison.CurrentCultureIgnoreCase))
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
                    if (!trigger.Data.Body.Equals(parameters.Body, StringComparison.CurrentCultureIgnoreCase))
                    {
                        return Task.FromResult(false);
                    }

                    break;
                case ReceivedEmailTriggerParameters.FieldComparer.Contains:
                    if (!trigger.Data.Body.Contains(parameters.Body, StringComparison.CurrentCultureIgnoreCase))
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