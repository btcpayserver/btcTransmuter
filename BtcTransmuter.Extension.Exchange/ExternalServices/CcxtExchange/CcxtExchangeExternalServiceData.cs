using System;

namespace BtcTransmuter.Extension.Exchange.ExternalServices.CcxtExchange
{
    public class ExchangeExternalServiceData
    {
        public string PublicKey { get; set; }
        public string PrivateKey { get; set; }
        public string PassPhrase { get; set; }
        public string ExchangeName { get; set; }
        public string OverrideUrl { get; set; }

        public DateTime? LastCheck { get; set; }
        public DateTime? PairedDate { get; set; }
    }
}