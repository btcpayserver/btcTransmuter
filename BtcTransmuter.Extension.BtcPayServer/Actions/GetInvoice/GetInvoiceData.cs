using System.ComponentModel.DataAnnotations;

namespace BtcTransmuter.Extension.BtcPayServer.Actions.GetInvoice
{
    public class GetInvoiceData
    {
        [Required]
        public string InvoiceId { get; set; }
    }
}