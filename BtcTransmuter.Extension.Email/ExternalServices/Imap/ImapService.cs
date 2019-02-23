using System;
using System.Threading.Tasks;
using BtcTransmuter.Abstractions.ExternalServices;
using BtcTransmuter.Abstractions.Helpers;
using BtcTransmuter.Data.Entities;
using MailKit.Net.Imap;
using MailKit.Net.Pop3;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;

namespace BtcTransmuter.Extension.Email.ExternalServices.Imap
{
    public class ImapService : BaseExternalService<ImapExternalServiceData>, IExternalServiceDescriptor
    {
        public const string ImapExternalServiceType = "ImapExternalService";
        public override string ExternalServiceType => ImapExternalServiceType;

        public string Name => "Imap External Service";
        public string Description => "Imap External Service to be able to analyze incoming email as a trigger";
        public string ViewPartial => "ViewImapExternalService";


        public ImapService() : base()
        {
        }

        public ImapService(ExternalServiceData data) : base(data)
        {
        }

        public Task<IActionResult> EditData(ExternalServiceData externalServiceData)
        {
            using (var scope = DependencyHelper.ServiceScopeFactory.CreateScope())
            {
                var identifier =externalServiceData.Id?? $"new_{Guid.NewGuid()}";
                if (string.IsNullOrEmpty(externalServiceData.Id))
                {
                    var memoryCache = scope.ServiceProvider.GetService<IMemoryCache>();
                    memoryCache.Set(identifier, externalServiceData, new MemoryCacheEntryOptions()
                    {
                        SlidingExpiration = TimeSpan.FromMinutes(60)
                    });
                }
                
                return Task.FromResult<IActionResult>(new RedirectToActionResult(nameof(ImapController.EditData),
                    "Imap", new
                    {
                        identifier
                    }));
            }
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