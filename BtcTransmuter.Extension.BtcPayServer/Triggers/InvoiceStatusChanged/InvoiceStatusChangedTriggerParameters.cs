using System.ComponentModel.DataAnnotations;

namespace BtcTransmuter.Extension.BtcPayServer.Triggers.InvoiceStatusChanged
{
    public class InvoiceStatusChangedTriggerParameters
    {
        public string Status { get; set; }
        [Display(Name = "Additional Status")]
        public string ExceptionStatus { get; set; }
    }
}