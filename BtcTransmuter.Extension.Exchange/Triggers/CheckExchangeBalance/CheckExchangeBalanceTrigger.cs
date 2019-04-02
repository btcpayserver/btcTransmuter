using BtcTransmuter.Abstractions.Triggers;

namespace BtcTransmuter.Extension.Exchange.Triggers.CheckExchangeBalance
{
    public class CheckExchangeBalanceTrigger : BaseTrigger<CheckExchangeBalanceTriggerData>
    {
        public new static readonly string Id = typeof(CheckExchangeBalanceTrigger).FullName;

    }
}