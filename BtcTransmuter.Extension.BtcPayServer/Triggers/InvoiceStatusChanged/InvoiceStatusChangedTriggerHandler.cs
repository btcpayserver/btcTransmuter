using System;
using System.Threading.Tasks;
using BtcTransmuter.Abstractions.Helpers;
using BtcTransmuter.Abstractions.Triggers;
using BtcTransmuter.Data.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;

namespace BtcTransmuter.Extension.BtcPayServer.Triggers.InvoiceStatusChanged
{
    public class InvoiceStatusChangedTriggerHandler : BaseTriggerHandler<InvoiceStatusChangedTriggerData,
        InvoiceStatusChangedTriggerParameters>, ITriggerDescriptor
    {
        public override string TriggerId => new InvoiceStatusChangedTrigger().Id;
        public string Name => "BtcPayServer Invoice Status Change";

        public string Description =>
            "Trigger a recipe by detecting a status change of an invoice in a btcpay external service.";

        public string ViewPartial => "ViewInvoiceStatusChangedTrigger";
        public Task<IActionResult> EditData(RecipeTrigger data)
        {
            using (var scope = DependencyHelper.ServiceScopeFactory.CreateScope())
            {
                var identifier = $"{Guid.NewGuid()}";
                var memoryCache = scope.ServiceProvider.GetService<IMemoryCache>();
                memoryCache.Set(identifier, data, new MemoryCacheEntryOptions()
                {
                    SlidingExpiration = TimeSpan.FromMinutes(60)
                });

                return Task.FromResult<IActionResult>(new RedirectToActionResult(
                    nameof(InvoiceStatusChangedController.EditData),
                    "InvoiceStatusChanged", new
                    {
                        identifier
                    }));
            }
        }


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