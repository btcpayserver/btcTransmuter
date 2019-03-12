using System.ComponentModel.DataAnnotations;

namespace BtcTransmuter.Extension.NBXplorer.Triggers.NBXplorerNewBlock
{
    public class NBXplorerNewBlockTriggerParameters
    {
        [Required]
        [Display(Name = "Crypto")]
        public string CryptoCode { get; set; }
    }
}