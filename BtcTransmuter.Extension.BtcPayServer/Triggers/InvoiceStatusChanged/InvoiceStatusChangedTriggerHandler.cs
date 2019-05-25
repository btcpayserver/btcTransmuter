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
            if (string.IsNullOrEmpty(parameters.Status))
            {
                return Task.FromResult(true);
            }

            var exceptionStatus = string.Empty;
            if (triggerData.Invoice.ExceptionStatus.Type == JTokenType.String)
            {
                var value = triggerData.Invoice.ExceptionStatus.Value<string>();
                if (!value.Equals("false", StringComparison.InvariantCultureIgnoreCase))
                {
                    exceptionStatus = value;
                }
            }

            var status = triggerData.Invoice.Status + (string.IsNullOrEmpty(exceptionStatus)? $"_{exceptionStatus}": "");
            return Task.FromResult(status.Equals(parameters.Status,
                StringComparison.InvariantCultureIgnoreCase));
        }
    }
}