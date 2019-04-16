using NBitpayClient;

namespace BtcTransmuter.Extension.BtcPayServer.HostedServices
{
    public class BtcPayInvoice : Invoice
    {
        public override bool ShouldSerializeId()
        {
            return true;
        }

        public override bool ShouldSerializeUrl()
        {
            return true;
        }

        public override bool ShouldSerializeStatus()
        {
            return true;
        }

        public override bool ShouldSerializeBtcPrice()
        {
            return true;
        }

        public override bool ShouldSerializeInvoiceTime()
        {
            return true;
        }

        public override bool ShouldSerializeExpirationTime()
        {
            return true;
        }

        public override bool ShouldSerializeBtcPaid()
        {
            return true;
        }

        public override bool ShouldSerializeBtcDue()
        {
            return true;
        }

        public override bool ShouldSerializeTransactions()
        {
            return true;
        }

        public override bool ShouldSerializeExRates()
        {
            return true;
        }

        public override bool ShouldSerializeExceptionStatus()
        {
            return true;
        }

        public override bool ShouldSerializePaymentUrls()
        {
            return true;
        }

        public override bool ShouldSerializeAddresses()
        {
            return true;
        }
        
        
    }
}