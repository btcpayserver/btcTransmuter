using System.ComponentModel.DataAnnotations;

namespace BtcTransmuter.Extension.Lightning.Actions.ConnectToLightningNode
{
    public class ConnectToLightningNodeData
    {
        [Required]
        public string NodeInfo { get; set; }
    }
}