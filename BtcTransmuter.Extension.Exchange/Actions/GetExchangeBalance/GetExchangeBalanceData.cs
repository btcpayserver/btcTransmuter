using System.ComponentModel.DataAnnotations;
using ExchangeSharp;

namespace BtcTransmuter.Extension.Exchange.Actions.GetExchangeBalance
{
    public class GetExchangeBalanceData
    {   
        [Required]
        public string Asset { get; set; }


    }
}