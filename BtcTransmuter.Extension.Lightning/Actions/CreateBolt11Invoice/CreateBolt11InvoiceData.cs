using System.ComponentModel.DataAnnotations;
using BTCPayServer.Lightning;

namespace BtcTransmuter.Extension.Email.Actions.CreateBolt11Invoice
{
    public class CreateBolt11InvoiceData
    {
        [Required]
        public string ExpiryMilliseconds { get; set; }
        [Required]
        public string Amount { get; set; }
        public LightMoneyUnit AmountMoneyUnit { get; set; } = LightMoneyUnit.BTC;
        public string Description { get; set; }
    }
}