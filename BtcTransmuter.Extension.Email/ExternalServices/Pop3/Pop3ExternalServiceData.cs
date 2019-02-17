using System;

namespace BtcTransmuter.Extension.Email.ExternalServices
{
    public class Pop3ExternalServiceData : BaseEmailServiceData
    {
        public DateTime? LastCheck { get; set; }
    }
}