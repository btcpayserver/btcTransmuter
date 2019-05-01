using BtcTransmuter.Extension.Lightning.Triggers.ReceivedLightningPayment;
using BtcTransmuter.Tests.Base;

namespace BtcTransmuter.Extension.Lightning.Tests
{
    public class ReceivedLightningPaymentTriggerHandlerTests : BaseTriggerTest<ReceivedLightningPaymentTriggerHandler,
        ReceivedLightningPaymentTrigger,
        ReceivedLightningPaymentTriggerParameters>
    {
        protected override ReceivedLightningPaymentTriggerHandler GetTriggerHandlerInstance(params object[] setupArgs)
        {
            return new ReceivedLightningPaymentTriggerHandler();
        }
    }
}