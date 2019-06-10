using System;
using System.Threading.Tasks;
using BtcTransmuter.Abstractions.Triggers;
using BtcTransmuter.Data.Entities;
using NBitpayClient;
using Newtonsoft.Json.Linq;

namespace BtcTransmuter.Extension.BtcPayServer.Triggers.InvoiceStatusChanged
{
    public class InvoiceStatusChangedTriggerHandler : BaseTriggerHandler<InvoiceStatusChangedTriggerData,
	    InvoiceStatusChangedTriggerParameters>
    {
        public override string TriggerId => new InvoiceStatusChangedTrigger().Id;
        public override string Name => "BtcPayServer Invoice Status Change";

        public override string Description =>
            "Trigger a recipe by detecting a status change of an invoice in a btcpay external service.";

        public override string ViewPartial => "ViewInvoiceStatusChangedTrigger";
        public override string ControllerName => "InvoiceStatusChanged";

        protected override Task<bool> IsTriggered(ITrigger trigger, RecipeTrigger recipeTrigger,
            InvoiceStatusChangedTriggerData triggerData,
            InvoiceStatusChangedTriggerParameters parameters)
        {
      
            var exceptionStatus =Invoice.EXSTATUS_FALSE;
            if (triggerData.Invoice.ExceptionStatus.Type == JTokenType.String)
            {
	            exceptionStatus = triggerData.Invoice.ExceptionStatus.Value<string>();
            }
            var status = triggerData.Invoice.Status;

            foreach (var condition in parameters.Conditions)
            {
	            if (condition.Status.Equals(status, StringComparison.InvariantCultureIgnoreCase) &&
	                condition.ExceptionStatuses.Contains(exceptionStatus))
	            {
		            return Task.FromResult(true);
	            }
            }

            return Task.FromResult(false);
        }
    }
}