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
    public class BtcPayServerService : BaseExternalService<BtcPayServerExternalServiceData>
    {
        public const string BtcPayServerServiceType = "BtcPayServerExternalService";
        public override string ExternalServiceType => BtcPayServerServiceType;

        public override string Name => "BtcPayServer External Service";
        public override string Description => "BtcPayServer External Service to be able to interact with its services";
        public override string ViewPartial => "ViewBtcPayServerExternalService";
        protected override string ControllerName => "BtcPayServer";


        public BtcPayServerService() : base()
        {
        }

        public BtcPayServerService(ExternalServiceData data) : base(data)
        {
        }

        public Bitpay ConstructClient()
        {
            try
            {
                var data = GetData();
                var seed = new Mnemonic(data.Seed);

                return new Bitpay(seed.DeriveExtKey().PrivateKey, data.Server);
            }
            catch (Exception)
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

                if (client == null || await CheckAccess())
                {
                    return null;
                }

                return (await client.RequestClientAuthorizationAsync("BtcTransmuter", Facade.Merchant))
                    .CreateLink(client.BaseUrl)
                    .ToString();
            }
            catch (Exception)
            {
                var data = GetData();
                return new Uri(data.Server, "api-tokens").ToString();
            }
        }
    }
}