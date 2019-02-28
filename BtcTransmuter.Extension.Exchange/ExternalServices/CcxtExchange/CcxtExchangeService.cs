using System.Linq;
using System.Threading.Tasks;
using BtcTransmuter.Abstractions.ExternalServices;
using BtcTransmuter.Data.Entities;
using ExchangeSharp;
using Pchp.Core;

namespace BtcTransmuter.Extension.Exchange.ExternalServices.CcxtExchange
{
    public class CcxtExchangeService : BaseExternalService<ExchangeExternalServiceData>
    {
        public const string ExchangeServiceType = "CcxtExchangeExternalService";
        public override string ExternalServiceType => ExchangeServiceType;

        public override string Name => "Ccxt Exchange External Service";
        public override string Description => "Integrate from a wide variety of cryptocurrency exchanges";
        public override string ViewPartial => "ViewCcxtExchangeExternalService";
        protected override string ControllerName => "CcxtExchange";


        public CcxtExchangeService() : base()
        {
        }

        public CcxtExchangeService(ExternalServiceData data) : base(data)
        {
        }

        public static string[] GetAvailableExchanges()
        {
            return  new ccxt.Exchange._statics().exchanges.GetArray().Values.Select(value => value.AsString(Context.CreateEmpty())).ToArray();
        }


        public ExchangeAPI ConstructClient()
        {
            var exchangeInstance = new ccxt.Exchange(Context.CreateEmpty());
            
            var e = new ccxt.therock(Context.CreateEmpty(),
            
            return null;
        }

        public async Task<bool> TestAccess()
        {
           
            return false;
        }
    }
}