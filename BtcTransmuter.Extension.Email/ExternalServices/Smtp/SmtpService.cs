using System.Threading.Tasks;
using BtcTransmuter.Abstractions.ExternalServices;
using BtcTransmuter.Data.Entities;
using MimeKit;
using SmtpClient = MailKit.Net.Smtp.SmtpClient;

namespace BtcTransmuter.Extension.Email.ExternalServices.Smtp
{
    public class SmtpService : BaseExternalService<SmtpExternalServiceData>
    {
        public const string SmtpExternalServiceType = "SmtpExternalService";
        public override string ExternalServiceType => SmtpExternalServiceType;

        public override string Name => "SMTP External Service";
        public override string Description => "SMTP External Service to be able to send emails as an action";
        public override string ViewPartial => "ViewSmtpExternalService";
        public override string ControllerName => "Smtp";


        public SmtpService() : base()
        {
        }

        public SmtpService(ExternalServiceData data) : base(data)
        {
        }

        public async Task SendEmail(MimeMessage message)
        {
            var data = GetData();
            using (var client = new SmtpClient())
            {
                client.ServerCertificateValidationCallback = (s, c, h, e) => true;
                await client.ConnectAsync(data.Server, data.Port, data.SSL);
                await client.AuthenticateAsync(data.Username, data.Password);
                await client.SendAsync(message);
                await client.DisconnectAsync(true);
            }
        }
    }
}