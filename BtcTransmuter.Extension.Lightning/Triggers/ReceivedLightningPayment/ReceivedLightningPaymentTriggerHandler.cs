using System.Threading.Tasks;
using BtcTransmuter.Abstractions.Triggers;
using BtcTransmuter.Data.Entities;

namespace BtcTransmuter.Extension.Lightning.Triggers.ReceivedLightningPayment
{
    public class ReceivedLightningPaymentTriggerHandler : BaseTriggerHandler<ReceivedLightningPaymentTrigger,
        ReceivedLightningPaymentTriggerParameters>
    {
        public override string TriggerId => new ReceivedLightningPaymentTrigger().Id;
        public override string Name => "Receive Lightning Payment";

        public  override string Description =>
            "Trigger a recipe by receiving a lightning payment through a connected lightning node";

        public override  string ViewPartial => "ViewReceivedLightningPaymentTrigger";
        public override string ControllerName => "ReceivedLightningPayment";

        protected override Task<bool> IsTriggered(ITrigger trigger, RecipeTrigger recipeTrigger,
            ReceivedLightningPaymentTrigger triggerData,
            ReceivedLightningPaymentTriggerParameters parameters)
        {
            

            return Task.FromResult(true);
        }
    }
}