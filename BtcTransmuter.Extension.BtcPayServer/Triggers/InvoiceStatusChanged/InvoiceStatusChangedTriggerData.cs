using BtcTransmuter.Abstractions.ExternalServices;
using BtcTransmuter.Extension.BtcPayServer.HostedServices;

namespace BtcTransmuter.Extension.BtcPayServer.Triggers.InvoiceStatusChanged
{
    public class InvoiceStatusChangedTriggerData: IUseExternalService
    {
        public BtcPayInvoice Invoice { get; set; }
        public string ExternalServiceId { get; set; }
    }
}