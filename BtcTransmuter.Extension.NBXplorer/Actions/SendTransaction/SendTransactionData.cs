using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BtcTransmuter.Extension.NBXplorer.Actions.SendTransaction
{
    public class SendTransactionData
    {
        [Display(Name = "Crypto")] [Required] public string CryptoCode { get; set; }
        [Display(Name = "Address")] public string Address { get; set; }

        [Display(Name = "Derivation Strategy")]
        public string DerivationStrategy { get; set; }

        [Display(Name = "Mnemonic Seed")] public string MnemonicSeed { get; set; }

        [Display(Name = "Passphrase")] public string Passphrase { get; set; }

        [Display(Name = "Private Key(WIF / Bitcoin Secret)")]
        public string WIF { get; set; }

        public List<TransactionOutput> Outputs { get; set; } = new List<TransactionOutput>();

        public class TransactionOutput
        {
            [Display(Name = "Destination Address")]
            [Required]
            public string DestinationAddress { get; set; }

            [Display(Name = "Amount")] [Required] public string Amount { get; set; }
        }
    }
}