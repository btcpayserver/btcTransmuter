using NBXplorer.Models;

namespace BtcTransmuter.Extension.NBXplorer.Triggers.NBXplorerNewBlock
{
    public class NBXplorerNewBlockTriggerData
    {
        public NewBlockEvent Event { get; set; }
        public string CryptoCode { get; set; }
    }
}