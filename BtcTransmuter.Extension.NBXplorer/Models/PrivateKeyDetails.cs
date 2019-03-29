using System.ComponentModel.DataAnnotations;

namespace BtcTransmuter.Extension.NBXplorer.Models
{
    public class PrivateKeyDetails
    {
        [Display(Name = "Mnemonic Seed")] public string MnemonicSeed { get; set; }

        [Display(Name = "Passphrase")] public string Passphrase { get; set; }

        [Display(Name = "Private Key(WIF / Bitcoin Secret)")]
        public string WIF { get; set; }
    }
}