using System;
using BtcTransmuter.Abstractions;
using BtcTransmuter.Abstractions.ExternalServices;
using BtcTransmuter.Data.Entities;

namespace BtcTransmuter.Extension.Email.ExternalServices
{
    public class Pop3Service: BaseExternalService<Pop3ExternalServiceData>
    {
        public override string ExternalServiceType => Pop3ExternalServiceDescriptor.Pop3ExternalServiceType;

        public Pop3Service(ExternalServiceData data) : base(data)
        {
        }

    }
}