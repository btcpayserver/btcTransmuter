using System;

namespace BtcTransmuter.Extension.Email.ExternalServices.Pop3
{
    public class Pop3ExternalServiceData : BaseEmailServiceData
    {
        public DateTime? LastCheck { get; set; }
    }
}