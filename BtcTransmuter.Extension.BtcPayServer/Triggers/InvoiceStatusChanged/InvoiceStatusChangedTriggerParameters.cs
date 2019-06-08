using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BtcTransmuter.Extension.BtcPayServer.Triggers.InvoiceStatusChanged
{
    public class InvoiceStatusChangedTriggerParameters
    {
        public IEnumerable<string> Status { get; set; }
        [Display(Name = "Additional Statuses")]
        public IEnumerable<string> ExceptionStatus { get; set; }
    }
}