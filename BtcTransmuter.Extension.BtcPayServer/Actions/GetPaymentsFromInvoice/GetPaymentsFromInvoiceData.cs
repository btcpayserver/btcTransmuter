using System.ComponentModel.DataAnnotations;

namespace BtcTransmuter.Extension.BtcPayServer.Actions.GetPaymentsFromInvoice
{
    public class GetPaymentsFromInvoiceData
    {
        [Required]
        public string InvoiceId { get; set; }
        [Required]
        public string CryptoCode { get; set; }
        public string PaymentType { get; set; }
    }
}