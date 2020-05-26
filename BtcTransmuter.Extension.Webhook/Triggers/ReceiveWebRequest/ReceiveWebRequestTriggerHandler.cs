using System;
using System.Threading.Tasks;
using BtcTransmuter.Abstractions.Triggers;
using BtcTransmuter.Data.Entities;

namespace BtcTransmuter.Extension.Webhook.Triggers.ReceiveWebRequest
{
    public class ReceiveWebRequestTriggerHandler : BaseTriggerHandler<ReceiveWebRequestTriggerData,
        ReceiveWebRequestTriggerParameters>
    {
        public override string TriggerId => new ReceiveWebRequestTrigger().Id;
        public override string Name => "Receive Web Request";

        public override string Description =>
            "Trigger a recipe when a specific web request is received";

        public override string ViewPartial => "ViewReceiveWebRequestTrigger";
        public override string ControllerName => "ReceiveWebRequest";


        protected override Task<bool> IsTriggered(ITrigger trigger, RecipeTrigger recipeTrigger,
            ReceiveWebRequestTriggerData triggerData,
            ReceiveWebRequestTriggerParameters parameters)
        {
            if (!string.IsNullOrEmpty(parameters.Method) && triggerData.Method != parameters.Method)
            {
                return Task.FromResult(false);
            }

            if (!triggerData.RelativeUrl.Equals(parameters.RelativeUrl, StringComparison.InvariantCultureIgnoreCase))
            {
                return Task.FromResult(false);
            }

            switch (parameters.BodyComparer)
            {
                case ReceiveWebRequestTriggerParameters.FieldComparer.None:
                    return Task.FromResult(true);
                case ReceiveWebRequestTriggerParameters.FieldComparer.Equals:
                    return Task.FromResult(triggerData.Body.Equals(parameters.Body));
                case ReceiveWebRequestTriggerParameters.FieldComparer.Contains:
                    return Task.FromResult(triggerData.Body.Contains(parameters.Body));
                default:
                    return Task.FromResult(true);
            }
        }
    }
}