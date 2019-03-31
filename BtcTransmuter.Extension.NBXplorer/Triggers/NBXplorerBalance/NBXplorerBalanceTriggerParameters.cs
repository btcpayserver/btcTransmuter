using System.ComponentModel.DataAnnotations;
using NBitcoin;

namespace BtcTransmuter.Extension.NBXplorer.Triggers.NBXplorerBalance
{
    public class NBXplorerBalanceTriggerParameters
    {
        [Display(Name = "Balance")]
        public decimal BalanceValue { get; set; }

        public MoneyUnit BalanceMoneyUnit { get; set; } = MoneyUnit.BTC;
        public BalanceComparer BalanceComparer { get; set; }

        public Money Balance => Money.FromUnit(BalanceValue, BalanceMoneyUnit);
    }
}