using System;
using System.Threading.Tasks;
using BtcTransmuter.Abstractions.ExternalServices;
using BtcTransmuter.Abstractions.Helpers;
using BtcTransmuter.Data.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using NBitcoin;
using NBitpayClient;

namespace BtcTransmuter.Extension.BtcPayServer.ExternalServices.BtcPayServer
{
    public class BtcPayServerService : BaseExternalService<BtcPayServerExternalServiceData>, IExternalServiceDescriptor
    {
        public const string BtcPayServerServiceType = "BtcPayServerExternalService";
        public override string ExternalServiceType => BtcPayServerServiceType;

        public string Name => "BtcPayServer External Service";
        public string Description => "BtcPayServer External Service to be able to interact with its services";
        public string ViewPartial => "ViewBtcPayServerExternalService";


        public BtcPayServerService() : base()
        {
        }

        public BtcPayServerService(ExternalServiceData data) : base(data)
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
                
                return Task.FromResult<IActionResult>(new RedirectToActionResult(nameof(BtcPayServerController.EditData),
                    "BtcPayServer", new
                    {
                        identifier
                    }));
            }
        }

        public Bitpay ConstructClient()
        {
            try
            {
                var data = GetData();
                var seed = new Mnemonic(data.Seed);

               return new Bitpay(seed.DeriveExtKey().PrivateKey, data.Server);
            }
            catch (Exception e)
            {
                return null;
            }
        }

        public async Task<bool> CheckAccess()
        {
            var client = ConstructClient();
            return client != null && await client.TestAccessAsync(Facade.Merchant);
        }

        public async Task<string> GetPairingUrl()
        {
            try
            {
                var client = ConstructClient();
                
                if (client == null  ||  await CheckAccess())
                {
                    return null;
                }

                return (await client.RequestClientAuthorizationAsync("BtcTransmuter", Facade.Merchant)).CreateLink(client.BaseUrl)
                    .ToString();
            }
            catch (Exception e)
            {
                var data = GetData();
                return new Uri(data.Server, "api-tokens").ToString();
            }

        }
    }
}