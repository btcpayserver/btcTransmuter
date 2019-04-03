using System.ComponentModel.DataAnnotations;

namespace BtcTransmuter.Extension.Exchange.Actions.GetExchangeRate
{
    public class GetExchangeRateData
    {   
        [Required]
        [Display(Name = "Market Symbol")]
        public string MarketSymbol { get; set; }
    }
}