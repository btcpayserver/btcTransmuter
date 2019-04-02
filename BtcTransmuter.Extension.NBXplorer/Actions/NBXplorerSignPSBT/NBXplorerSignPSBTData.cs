using System.ComponentModel.DataAnnotations;

namespace BtcTransmuter.Extension.NBXplorer.Actions.NBXplorerSignPSBT
{
    public class NBXplorerSignPSBTData
    {
        [Required]
        public string PSBT { get; set; }
    }
}