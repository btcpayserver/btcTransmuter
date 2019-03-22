using System.ComponentModel.DataAnnotations;
using NBitcoin;

namespace BtcTransmuter.Extension.Lightning.Actions.OpenLightningChannel
{
    public class OpenLightningChannelData
    {
        [Required] public string Amount { get; set; }
        public MoneyUnit AmountMoneyUnit { get; set; } = MoneyUnit.BTC;
        [Required] public string NodeInfo { get; set; }
    }
}