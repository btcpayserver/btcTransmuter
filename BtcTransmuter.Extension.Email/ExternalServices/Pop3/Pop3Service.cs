using System;
using System.Threading.Tasks;
using BtcTransmuter.Abstractions.ExternalServices;
using BtcTransmuter.Data.Entities;
using MailKit.Net.Pop3;

namespace BtcTransmuter.Extension.Email.ExternalServices.Pop3
{
    public class Pop3Service : BaseExternalService<Pop3ExternalServiceData>
    {
        public const string Pop3ExternalServiceType = "Pop3ExternalService";
        public override string ExternalServiceType => Pop3ExternalServiceType;

        public override string Name => "Pop3 External Service";
        public override string Description => "Pop3 External Service to be able to analyze incoming email as a trigger";
        public override  string ViewPartial => "ViewPop3ExternalService";

        public override string ControllerName => "Pop3";


        public Pop3Service() : base()
        {
        }

        public Pop3Service(ExternalServiceData data) : base(data)
        {
        }

        public async Task<Pop3Client> CreateClientAndConnect()
        {
            try
            {
                var pop3Client = new Pop3Client();
                var data = GetData();
                pop3Client.ServerCertificateValidationCallback = (s,c,h,e) => true;

                await pop3Client.ConnectAsync(data.Server, data.Port, data.SSL);
                
                await pop3Client.AuthenticateAsync(data.Username, data.Password);

                return pop3Client.IsConnected && pop3Client.IsAuthenticated? pop3Client : null;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}