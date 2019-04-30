using BtcTransmuter.Extension.Exchange.Actions.GetExchangeRate;
using BtcTransmuter.Tests.Base;
using ExchangeSharp;

namespace BtcTransmuter.Extension.Exchange.Tests
{
    public class GetExchangeRateDataActionHandlerTests :
        BaseActionTest<GetExchangeRateDataActionHandler, GetExchangeRateData, ExchangeTicker>
    {
        protected override GetExchangeRateDataActionHandler GetActionHandlerInstance(params object[] setupArgs)
        {
            return new GetExchangeRateDataActionHandler();
        }
    }
}