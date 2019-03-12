using System.ComponentModel.DataAnnotations;
using ExchangeSharp;

namespace BtcTransmuter.Extension.Exchange.Actions.PlaceOrder
{
    public class PlaceOrderData
    {
        [Required]
        [Display(Name = "Market Symbol")]
        public string MarketSymbol { get; set; }
        [Required]
        public string Amount { get; set; }
        public string Price { get; set; }

        [Display(Name = "Stop Price")]
        public string StopPrice { get; set; }
        [Required]
        
        [Display(Name = "Is Buy")]
        public bool IsBuy { get; set; }
        [Required]
        
        [Display(Name = "Is Margin")]
        public bool IsMargin { get; set; }
        [Required]
        [Display(Name = "Order type")]
        public OrderType OrderType { get; set; }


    }
}