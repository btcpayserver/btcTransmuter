using System;
using System.Threading.Tasks;
using BtcTransmuter.Abstractions.ExternalServices;
using BtcTransmuter.Abstractions.Helpers;
using BtcTransmuter.Data.Entities;
using ExchangeSharp;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;

namespace BtcTransmuter.Extension.Exchange.ExternalServices.Exchange
{
    public class ExchangeService : BaseExternalService<ExchangeExternalServiceData>, IExternalServiceDescriptor
    {
        public const string ExchangeServiceType = "ExchangeExternalService";
        public override string ExternalServiceType => ExchangeServiceType;

        public override string Name => "Exchange External Service";
        public override  string Description => "Integrate from a wide variety of cryptocurrency exchanges";
        public override  string ViewPartial => "ViewExchangeExternalService";


        public ExchangeService() : base()
        {
        }

        public ExchangeService(ExternalServiceData data) : base(data)
        {
        }

        public override  Task<IActionResult> EditData(ExternalServiceData externalServiceData)
        {
            using (var scope = DependencyHelper.ServiceScopeFactory.CreateScope())
            {
                var identifier = externalServiceData.Id ?? $"new_{Guid.NewGuid()}";
                if (string.IsNullOrEmpty(externalServiceData.Id))
                {
                    var memoryCache = scope.ServiceProvider.GetService<IMemoryCache>();
                    memoryCache.Set(identifier, externalServiceData, new MemoryCacheEntryOptions()
                    {
                        SlidingExpiration = TimeSpan.FromMinutes(60)
                    });
                }

                return Task.FromResult<IActionResult>(new RedirectToActionResult(nameof(ExchangeController.EditData),
                    "Exchange", new
                    {
                        identifier
                    }));
            }
        }

        public static IExchangeAPI[] GetAvailableExchanges()
        {
            return ExchangeAPI.GetExchangeAPIs();
        }


        public ExchangeAPI ConstructClient()
        {
            var data = GetData();

            var result = ExchangeAPI.GetExchangeAPI(data.ExchangeName);
            if (result is ExchangeAPI api)
            {
                if (!string.IsNullOrEmpty(data.OverrideUrl))
                {
                    api.BaseUrl = data.OverrideUrl;
                }


                api.LoadAPIKeysUnsecure(data.PublicKey, data.PrivateKey, data.PassPhrase);
                return api;
            }

            return null;
        }

        public async Task<bool> TestAccess()
        {
            var client = ConstructClient();
            if (client == null)
            {
                return false;
            }

            try
            {
                _ = await client.GetAmountsAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}