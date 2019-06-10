using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using BtcTransmuter.Abstractions;

namespace BtcTransmuter.Extension.BtcPayServer.Triggers.InvoiceStatusChanged
{
    public class InvoiceStatusChangedTriggerParameters
    {
        public List<InvoiceStatusChangeCondition> Conditions { get; set; } = new List<InvoiceStatusChangeCondition>();
    }

    public class InvoiceStatusChangeCondition
    {
		[Required]
	    public string Status { get; set; }
		[Display(Name = "Additional Statuses")]
	    [Required]
		[EnsureMinimumElements(1, ErrorMessage = "At least one additional status is required")]
		public List<string> ExceptionStatuses { get; set; }
    }
}