using System;
using System.ComponentModel.DataAnnotations;

namespace BtcTransmuter.Extension.Lightning.ExternalServices.LightningNode
{
    public class LightningNodeExternalServiceData
    {
        [Display(Name = "Connection String")]
        [Required]
        public string ConnectionString { get; set; }

        [Required]
        public string CryptoCode { get; set; }
    }
}