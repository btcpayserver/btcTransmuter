using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using NBXplorer.Models;

namespace BtcTransmuter.Extension.NBXplorer.Triggers.NBXplorerNewTransaction
{
    public class NBXplorerNewTransactionTriggerParameters
    {
        [Display(Name = "Confirmations Required")]
        public int ConfirmationsRequired { get; set; } = 0;

        public List<TransactionResult> Transactions { get; set; }
    }
}