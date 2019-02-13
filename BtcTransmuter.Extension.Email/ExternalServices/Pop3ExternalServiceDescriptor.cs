using BtcTransmuter.Abstractions.ExternalServices;

namespace BtcTransmuter.Extension.Email.ExternalServices
{
    public class Pop3ExternalServiceDescriptor : IExternalServiceDescriptor
    {
        public const string Pop3ExternalServiceType = "Pop3ExternalService";
        public string ExternalServiceType => Pop3ExternalServiceType;
        public string Name => "Pop3 External Service";
        public string Description => "Pop3 External Service to be able to analyze incoming email as a trigger";
        public string ViewPartial => "ViewPop3ExternalService";
    }
}