using System;
using System.ComponentModel.DataAnnotations;

namespace BtcTransmuter.Extension.Email.ExternalServices.Pop3
{
    public class BtcPayServerExternalServiceData
    {
        [Required]
        public Uri Server { get; set; }

        public string Seed { get; set; }
            
        public DateTime? LastCheck { get; set; }
    }
}