using NBXplorer.Models;
using Newtonsoft.Json;

namespace BtcTransmuter.Extension.NBXplorer.Triggers.NBXplorerNewTransaction
{
    public class NBXplorerNewTransactionTriggerData
    {
        [JsonConverter(typeof(NBXplorerBtcTransmuterExtension.TrackedSourceJsonConverter))]
        public NewTransactionEvent Event { get; set; }
        public string CryptoCode { get; set; }
    }
}