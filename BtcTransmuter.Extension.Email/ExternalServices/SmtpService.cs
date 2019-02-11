using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using BtcTransmuter.Abstractions;
using BtcTransmuter.Abstractions.ExternalServices;
using BtcTransmuter.Data.Entities;

namespace BtcTransmuter.Extension.Email.ExternalServices
{
    public class SmtpService : BaseExternalService<Pop3ExternalServiceData>
    {
        protected override string ExternalServiceType => SmtpExternalServiceDescriptor.SmtpExternalServiceType;

        public SmtpService(ExternalServiceData data) : base(data)
        {
        }

        public async Task SendEmail(MailMessage message)
        {
            using (var client = new SmtpClient()
            {
                Port = Data.Port,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Host = Data.Server,
                Credentials = new NetworkCredential(Data.Username, Data.Password),
                EnableSsl = Data.SSL
            })
            {
                await client.SendMailAsync(message);
            }
        }
    }
}