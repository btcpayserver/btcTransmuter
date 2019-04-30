using BtcTransmuter.Abstractions.Actions;
using BtcTransmuter.Extension.Exchange.Actions.GetExchangeBalance;
using BtcTransmuter.Tests.Base;
using Microsoft.AspNetCore.Mvc.Testing;

namespace BtcTransmuter.Extension.Exchange.Tests
{
    public class GetExchangeBalanceDataActionHandlerTests :
        BaseActionTest<BaseActionHandler<GetExchangeBalanceData, decimal>, GetExchangeBalanceData, decimal>
    {
        protected override BaseActionHandler<GetExchangeBalanceData, decimal> GetActionHandlerInstance(params object[] setupArgs)
        {
            return new GetExchangeBalanceDataActionHandler();
        }
    }
}