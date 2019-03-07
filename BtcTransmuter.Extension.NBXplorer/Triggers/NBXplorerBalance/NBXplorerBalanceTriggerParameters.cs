using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using NBitcoin;
using NBXplorer.Models;

namespace BtcTransmuter.Extension.NBXplorer.Triggers.NBXplorerBalance
{
    public class NBXplorerBalanceTriggerParameters
    {
        [Required] public string CryptoCode { get; set; }
        public string Address { get; set; }
        public string DerivationStrategy { get; set; }
        
        public decimal BalanceValue { get; set; }
        public MoneyUnit BalanceMoneyUnit { get; set; }
        public BalanceComparer BalanceComparer { get; set; }

        public Money Balance => Money.FromUnit(BalanceValue, BalanceMoneyUnit);
    }

    public enum BalanceComparer
    {
        LessThanOrEqual,
        GreaterThanOrEqual
    }
}