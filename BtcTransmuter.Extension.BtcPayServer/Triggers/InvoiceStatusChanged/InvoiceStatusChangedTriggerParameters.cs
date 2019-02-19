using System.ComponentModel.DataAnnotations;

namespace BtcTransmuter.Extension.BtcPayServer.Triggers.InvoiceStatusChanged
{
    public class InvoiceStatusChangedTriggerParameters
    {
        public string Status { get; set; }
    }
}