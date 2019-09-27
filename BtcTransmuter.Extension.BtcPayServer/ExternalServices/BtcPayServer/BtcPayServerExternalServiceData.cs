using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BtcTransmuter.Extension.BtcPayServer.ExternalServices.BtcPayServer
{
    public class BtcPayServerExternalServiceData
    {
        [Required][Display(Name = "BtcPay Host Url")]
        [Validation.Uri] public Uri Server { get; set; }

        public string Seed { get; set; }

        public DateTime? LastCheck { get; set; }
        public DateTime? PairedDate { get; set; }
        public Dictionary<string, string> MonitoredInvoiceStatuses { get; set; } = new Dictionary<string, string>();
    }
}