using System;
using System.Threading.Tasks;
using BtcTransmuter.Abstractions.ExternalServices;
using BtcTransmuter.Data.Entities;
using ExchangeSharp;

namespace BtcTransmuter.Extension.Exchange.ExternalServices.Exchange
{
    public class ExchangeService : BaseExternalService<ExchangeExternalServiceData>
    {
        public const string ExchangeServiceType = "ExchangeExternalService";
        public override string ExternalServiceType => ExchangeServiceType;

        public override string Name => "Exchange External Service";
        public override string Description => "Integrate from a wide variety of cryptocurrency exchanges";
        public override string ViewPartial => "ViewExchangeExternalService";
        public override string ControllerName => "Exchange";


        public ExchangeService() : base()
        {
        }

        public ExchangeService(ExternalServiceData data) : base(data)
        {
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