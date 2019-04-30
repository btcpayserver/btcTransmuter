using System.Collections.Generic;
using BtcTransmuter.Extension.BtcPayServer.Actions.GetPaymentsFromInvoice;
using BtcTransmuter.Tests.Base;
using NBitpayClient;

namespace BtcTransmuter.Extension.BtcPayServer.Tests
{
    public class
        GetPaymentsFromInvoiceDataActionHandlerTests : BaseActionTest<GetPaymentsFromInvoiceDataActionHandler, GetPaymentsFromInvoiceData, List<InvoicePaymentInfo>>
    {
        protected override GetPaymentsFromInvoiceDataActionHandler GetActionHandlerInstance(params object[] setupArgs)
        {
            return new GetPaymentsFromInvoiceDataActionHandler();
        }
    }
}