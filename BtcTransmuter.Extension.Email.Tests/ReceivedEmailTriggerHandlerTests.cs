using BtcTransmuter.Extension.Email.Triggers.ReceivedEmail;
using BtcTransmuter.Tests.Base;

namespace BtcTransmuter.Extension.Email.Tests
{
    public class ReceivedEmailTriggerHandlerTests : BaseTriggerTest<ReceivedEmailTriggerHandler,
        ReceivedEmailTriggerData,
        ReceivedEmailTriggerParameters>
    {
        protected override ReceivedEmailTriggerHandler GetTriggerHandlerInstance(params object[] setupArgs)
        {
            return new ReceivedEmailTriggerHandler();
        }
    }
}