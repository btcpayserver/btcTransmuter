using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BtcTransmuter.Extension.BtcPayServer.ExternalServices.BtcPayServer
{
    public class BtcPayServerExternalServiceData
    {
        [Required] public Uri Server { get; set; }

        public string Seed { get; set; }

        public DateTime? LastCheck { get; set; }
        public Dictionary<string, string> MonitoredInvoiceStatuses { get; set; }
    }
}