using BtcTransmuter.Abstractions.ExternalServices;
using NBitpayClient;

namespace BtcTransmuter.Extension.BtcPayServer.Triggers.InvoiceStatusChanged
{
    public class InvoiceStatusChangedTriggerData: IUseExternalService
    {
        public Invoice Invoice { get; set; }
        public string ExternalServiceId { get; set; }
    }
}