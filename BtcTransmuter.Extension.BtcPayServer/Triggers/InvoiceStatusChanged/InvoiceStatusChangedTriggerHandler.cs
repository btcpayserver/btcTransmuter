using System;
using System.Threading.Tasks;
using BtcTransmuter.Abstractions.Triggers;
using BtcTransmuter.Data.Entities;
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
      
            var exceptionStatus = string.Empty;
            if (triggerData.Invoice.ExceptionStatus.Type == JTokenType.String)
            {
                var value = triggerData.Invoice.ExceptionStatus.Value<string>();
                if (!value.Equals("false", StringComparison.InvariantCultureIgnoreCase))
                {
                    exceptionStatus = value;
                }
            }

            var status = triggerData.Invoice.Status;

            if (string.IsNullOrEmpty(parameters.Status) && string.IsNullOrEmpty(parameters.ExceptionStatus))
            {
                return Task.FromResult(true);
            }
            if (string.IsNullOrEmpty(parameters.Status) && !string.IsNullOrEmpty(parameters.ExceptionStatus))
            {
                return Task.FromResult(parameters.ExceptionStatus.Equals(exceptionStatus)); 
            }
            if (!string.IsNullOrEmpty(parameters.Status) && string.IsNullOrEmpty(parameters.ExceptionStatus))
            {
                return Task.FromResult(status.Equals(parameters.Status,
                    StringComparison.InvariantCultureIgnoreCase));
            }
            if (!string.IsNullOrEmpty(parameters.Status) && !string.IsNullOrEmpty(parameters.ExceptionStatus))
            {
                return Task.FromResult(status.Equals(parameters.Status,
                                           StringComparison.InvariantCultureIgnoreCase) && exceptionStatus.Equals(parameters.ExceptionStatus,
                                           StringComparison.InvariantCultureIgnoreCase));
            }

            return Task.FromResult(false);
        }
    }
}