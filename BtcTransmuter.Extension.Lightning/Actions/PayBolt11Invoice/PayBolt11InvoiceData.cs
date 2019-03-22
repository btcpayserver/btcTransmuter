using System.ComponentModel.DataAnnotations;

namespace BtcTransmuter.Extension.Lightning.Actions.PayBolt11Invoice
{
    public class PayBolt11InvoiceData
    {
        [Required]
        public string Bolt11 { get; set; }
    }
}