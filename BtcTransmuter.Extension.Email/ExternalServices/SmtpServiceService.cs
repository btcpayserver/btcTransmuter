using BtcTransmuter.Abstractions;

namespace BtcTransmuter.Extension.Email.ExternalServices
{
    public class SmtpServiceService: BaseExternalService<Pop3ExternalServiceData>
    {
        protected override string ExternalServiceType => EmailBtcTransmuterExtension.SmtpExternalServiceType;

        public SmtpServiceService(ExternalServiceData data) : base(data)
        {
        }

    }
}