using System;
using BtcTransmuter.Abstractions;

namespace BtcTransmuter.Extension.Email.ExternalServices
{
    public class Pop3Service: BaseExternalService<Pop3ExternalServiceData>
    {
        protected override string ExternalServiceType => EmailBtcTransmuterExtension.Pop3ExternalServiceType;

        public Pop3Service(ExternalServiceData data) : base(data)
        {
        }

    }
}