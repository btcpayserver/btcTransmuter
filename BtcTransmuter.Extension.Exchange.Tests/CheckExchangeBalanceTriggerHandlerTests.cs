using BtcTransmuter.Extension.Exchange.Triggers.CheckExchangeBalance;
using BtcTransmuter.Tests.Base;

namespace BtcTransmuter.Extension.Exchange.Tests
{
    public class CheckExchangeBalanceTriggerHandlerTests :
        BaseTriggerTest<CheckExchangeBalanceTriggerHandler, CheckExchangeBalanceTriggerData,
            CheckExchangeBalanceTriggerParameters>
    {
        protected override CheckExchangeBalanceTriggerHandler GetTriggerHandlerInstance(params object[] setupArgs)
        {
            return new CheckExchangeBalanceTriggerHandler();
        }
    }
}