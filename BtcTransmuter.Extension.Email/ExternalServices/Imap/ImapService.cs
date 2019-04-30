using System;
using System.Threading.Tasks;
using BtcTransmuter.Abstractions.ExternalServices;
using BtcTransmuter.Data.Entities;
using MailKit.Net.Imap;

namespace BtcTransmuter.Extension.Email.ExternalServices.Imap
{
    public class ImapService : BaseExternalService<ImapExternalServiceData>
    {
        public const string ImapExternalServiceType = "ImapExternalService";
        public override string ExternalServiceType => ImapExternalServiceType;

        public override string Name => "Imap External Service";
        public override string Description => "Imap External Service to be able to analyze incoming email as a trigger";
        public override string ViewPartial => "ViewImapExternalService";
        public override string ControllerName => "Imap";


        public ImapService() : base()
        {
        }

        public ImapService(ExternalServiceData data) : base(data)
        {
        }


        public async Task<ImapClient> CreateClientAndConnect()
        {
            try
            {
                var client = new ImapClient();
                var data = GetData();
                client.ServerCertificateValidationCallback = (s,c,h,e) => true;

                await client.ConnectAsync(data.Server, data.Port, data.SSL);
                
                await client.AuthenticateAsync(data.Username, data.Password);

                return client.IsConnected && client.IsAuthenticated? client : null;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}