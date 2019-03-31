using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BtcTransmuter.Extension.NBXplorer.Actions.SendTransaction
{
    public class SendTransactionData
    {
        public List<TransactionOutput> Outputs { get; set; } = new List<TransactionOutput>();

        public class TransactionOutput
        {
            [Display(Name = "Destination Address")]
            [Required]
            public string DestinationAddress { get; set; }

            [Display(Name = "Amount")] [Required] public string Amount { get; set; }


            [Display(Name = "Subtract fees from this output")]
            public bool SubtractFeesFromOutput { get; set; }
        }
    }
}