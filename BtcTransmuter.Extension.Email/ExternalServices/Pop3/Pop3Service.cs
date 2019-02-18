using System;
using System.Threading.Tasks;
using BtcTransmuter.Abstractions.ExternalServices;
using BtcTransmuter.Data.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Pop3;

namespace BtcTransmuter.Extension.Email.ExternalServices
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
                await pop3Client.ConnectAsync(Data.Server, Data.Username,
                    Data.Password, Data.SSL);

                return pop3Client.IsConnected ? pop3Client : null;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}