using System.ComponentModel.DataAnnotations;

namespace BtcTransmuter.Extension.NBXplorer.Triggers.NBXplorerNewTransaction
{
    public class NBXplorerNewTransactionTriggerParameters
    {
        [Required]
        public string CryptoCode { get; set; }
        public string Address { get; set; }
        public string DerivationStrategy { get; set; }
    }
}