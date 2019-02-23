using System;

namespace BtcTransmuter.Extension.Email.ExternalServices.Imap
{
    public class ImapExternalServiceData : BaseEmailServiceData
    {
        public DateTime? LastCheck { get; set; }
        public DateTime PairedDate { get; set; }
    }
}