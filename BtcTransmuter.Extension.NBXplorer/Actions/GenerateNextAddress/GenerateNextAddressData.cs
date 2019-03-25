using System.ComponentModel.DataAnnotations;

namespace BtcTransmuter.Extension.NBXplorer.Actions.GenerateNextAddress
{
    public class GenerateNextAddressData
    {
        [Display(Name = "Crypto")] [Required] public string CryptoCode { get; set; }

        [Display(Name = "Derivation Strategy")]
        [Required]
        public string DerivationStrategy { get; set; }
    }
}