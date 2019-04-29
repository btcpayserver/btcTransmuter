using BtcTransmuter.Abstractions.ExternalServices;
using BTCPayServer.Lightning;

namespace BtcTransmuter.Extension.Lightning.Triggers.ReceivedLightningPayment
{
    public class ReceivedLightningPaymentTriggerData: IUseExternalService
    {
        public LightningInvoice LightningInvoice { get; set; }
        public string ExternalServiceId { get; set; }
    }
}