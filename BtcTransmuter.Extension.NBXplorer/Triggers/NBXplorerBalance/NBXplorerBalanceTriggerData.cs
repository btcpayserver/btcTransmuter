using NBitcoin;
using NBXplorer.Models;
using Newtonsoft.Json;

namespace BtcTransmuter.Extension.NBXplorer.Triggers.NBXplorerBalance
{
    public class NBXplorerBalanceTriggerData
    {
        public Money Balance { get; set; }
        public string CryptoCode { get; set; }
        public TrackedSource TrackedSource { get; set; }
    }
}