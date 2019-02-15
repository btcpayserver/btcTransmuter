using BtcTransmuter.Abstractions.ExternalServices;
using BtcTransmuter.Data.Entities;

namespace BtcTransmuter.Extension.Email.ExternalServices
{
    public class Pop3Service: BaseExternalService<Pop3ExternalServiceData>, IExternalServiceDescriptor
    {
        public const string Pop3ExternalServiceType = "Pop3ExternalService";
        public override string ExternalServiceType => Pop3ExternalServiceType;

        public string Name => "Pop3 External Service";
        public string Description => "Pop3 External Service to be able to analyze incoming email as a trigger";
        public string ViewPartial => "ViewPop3ExternalService";

        public Pop3Service(ExternalServiceData data) : base(data)
        {
        }
    }
}