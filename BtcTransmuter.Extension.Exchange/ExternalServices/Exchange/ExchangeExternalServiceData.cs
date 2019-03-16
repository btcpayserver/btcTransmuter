using System;
using System.ComponentModel.DataAnnotations;

namespace BtcTransmuter.Extension.Exchange.ExternalServices.Exchange
{
    public class ExchangeExternalServiceData
    {
        [Display(Name = "API/Public Key")]
        public string PublicKey { get; set; }

        [Display(Name = "API Secret/Private key")]
        public string PrivateKey { get; set; }

        [Display(Name = "Passphrase")]
        public string PassPhrase { get; set; }

        [Display(Name = "Exchange")]
        public string ExchangeName { get; set; }

        [Display(Name = "Exchange Override Url (for sandbox/test environments)")]
        public string OverrideUrl { get; set; }

        public DateTime? LastCheck { get; set; }
        public DateTime? PairedDate { get; set; }
    }
}