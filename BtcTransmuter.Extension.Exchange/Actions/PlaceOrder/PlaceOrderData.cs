using System.ComponentModel.DataAnnotations;
using ExchangeSharp;

namespace BtcTransmuter.Extension.Exchange.Actions.PlaceOrder
{
    public class PlaceOrderData
    {
        [Required]
        public string MarketSymbol { get; set; }

        public string Amount { get; set; }

        public string Price { get; set; }

        public string StopPrice { get; set; }

        public bool IsBuy { get; set; }

        public bool IsMargin { get; set; }

        public OrderType OrderType { get; set; }


    }
}