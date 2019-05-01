using BtcTransmuter.Abstractions.Actions;
using BtcTransmuter.Extension.Exchange.Actions.GetExchangeBalance;
using BtcTransmuter.Tests.Base;
using Microsoft.AspNetCore.Mvc.Testing;

namespace BtcTransmuter.Extension.Exchange.Tests
{
    public class GetExchangeBalanceDataActionHandlerTests :BaseActionTest<GetExchangeBalanceDataActionHandler, GetExchangeBalanceData, decimal>
    {
    }
}