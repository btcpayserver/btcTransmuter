using BtcTransmuter.Extension.BtcPayServer.Triggers.InvoiceStatusChanged;
using BtcTransmuter.Tests.Base;

namespace BtcTransmuter.Extension.BtcPayServer.Tests
{
    public class
        InvoiceStatusChangedTriggerHandlerTests : BaseTriggerTest<InvoiceStatusChangedTriggerHandler,InvoiceStatusChangedTriggerData,
            InvoiceStatusChangedTriggerParameters>
    {
        protected override InvoiceStatusChangedTriggerHandler GetTriggerHandlerInstance(params object[] setupArgs)
        {
            return new InvoiceStatusChangedTriggerHandler();
        }
    }
}