namespace BtcTransmuter.Extension.Exchange.Triggers.CheckExchangeBalance
{
    public class CheckExchangeBalanceTriggerData
    {
        public decimal Balance { get; set; }
        public string Asset { get; set; }
        public string ExternalServiceId { get; set; }
    }
}