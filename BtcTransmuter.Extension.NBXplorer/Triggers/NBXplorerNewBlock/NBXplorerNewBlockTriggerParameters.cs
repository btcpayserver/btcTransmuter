using System.ComponentModel.DataAnnotations;

namespace BtcTransmuter.Extension.NBXplorer.Triggers.NBXplorerNewBlock
{
    public class NBXplorerNewBlockTriggerParameters
    {
        [Required]
        public string CryptoCode { get; set; }
    }
}