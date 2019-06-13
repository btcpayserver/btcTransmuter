using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BtcTransmuter.Extension.NBXplorer.Actions.SendTransaction
{
	public class SendTransactionData
	{
		public List<TransactionOutput> Outputs { get; set; } = new List<TransactionOutput>();

		[Range(1, int.MaxValue)]
		[Display(Name = "Fee rate (satoshi per byte)")]
		public int? FeeSatoshiPerByte { get; set; }

		[Display(Name = "Flat fee")] public decimal? Fee { get; set; }

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