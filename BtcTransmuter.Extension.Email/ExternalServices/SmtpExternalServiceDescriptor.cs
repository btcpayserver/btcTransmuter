using BtcTransmuter.Abstractions.ExternalServices;

namespace BtcTransmuter.Extension.Email.ExternalServices
{
    public class SmtpExternalServiceDescriptor : IExternalServiceDescriptor
    {
        public const string SmtpExternalServiceType = "SmtpExternalService";
        public string ExternalServiceType => SmtpExternalServiceType;
        public string Name => "SMTP External Service";
        public string Description => "SMTP External Service to be able to send emails as an action";
    }
}