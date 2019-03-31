using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using BtcTransmuter.Extension.NBXplorer.Models;

namespace BtcTransmuter.Extension.Lightning.ExternalServices.NBXplorerWallet
{
    public class NBXplorerWalletExternalServiceData
    {
        [Display(Name = "Crypto")] [Required] public string CryptoCode { get; set; }
        [Display(Name = "Address")] public string Address { get; set; }

        [Display(Name = "Derivation Strategy")]
        public string DerivationStrategy { get; set; }

        public List<PrivateKeyDetails> PrivateKeys { get; set; } = new List<PrivateKeyDetails>();
    }
}