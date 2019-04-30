using System;
using System.Threading.Tasks;
using BtcTransmuter.Abstractions.Triggers;
using BtcTransmuter.Data.Entities;

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

            return Task.FromResult(triggerData.Invoice.Status.Equals(parameters.Status,
                StringComparison.InvariantCultureIgnoreCase));
        }
    }
}