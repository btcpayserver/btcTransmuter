using System;
using System.Threading.Tasks;
using BtcTransmuter.Abstractions.ExternalServices;
using BtcTransmuter.Abstractions.Helpers;
using BtcTransmuter.Data.Entities;
using MailKit.Net.Pop3;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;

namespace BtcTransmuter.Extension.Email.ExternalServices.Pop3
{
    public class Pop3Service : BaseExternalService<Pop3ExternalServiceData>, IExternalServiceDescriptor
    {
        public const string Pop3ExternalServiceType = "Pop3ExternalService";
        public override string ExternalServiceType => Pop3ExternalServiceType;

        public string Name => "Pop3 External Service";
        public string Description => "Pop3 External Service to be able to analyze incoming email as a trigger";
        public string ViewPartial => "ViewPop3ExternalService";


        public Pop3Service() : base()
        {
        }

        public Pop3Service(ExternalServiceData data) : base(data)
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
                
                return Task.FromResult<IActionResult>(new RedirectToActionResult(nameof(Pop3Controller.EditData),
                    "Pop3", new
                    {
                        identifier
                    }));
            }
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