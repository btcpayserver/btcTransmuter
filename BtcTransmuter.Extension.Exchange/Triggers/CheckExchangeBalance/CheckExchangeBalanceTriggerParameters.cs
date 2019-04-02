using System.ComponentModel.DataAnnotations;

namespace BtcTransmuter.Extension.Exchange.Triggers.CheckExchangeBalance
{
    public class CheckExchangeBalanceTriggerParameters
    {
        [Display(Name = "Balance")] [Required] public decimal BalanceValue { get; set; }

        [Required] public string Asset { get; set; }
        public BalanceComparer BalanceComparer { get; set; }
    }
}