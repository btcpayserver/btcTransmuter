using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BtcTransmuter.Abstractions.Helpers;
using BtcTransmuter.Abstractions.Recipes;
using BtcTransmuter.Abstractions.Triggers;
using BtcTransmuter.Data.Entities;
using BtcTransmuter.Data.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;

namespace BtcTransmuter.Extension.Webhook.Triggers.ReceiveWebRequest
{
    public class ReceiveWebRequestTriggerHandler : BaseTriggerHandler<ReceiveWebRequestTriggerData,
        ReceiveWebRequestTriggerParameters>, ITriggerDescriptor
    {
        public override string TriggerId => new ReceiveWebRequestTrigger().Id;
        public string Name => "Receive Web Request";

        public string Description =>
            "Trigger a recipe when a specific web request is received";

        public string ViewPartial => "ViewReceiveWebRequestTrigger";

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
                    nameof(ReceiveWebRequestController.EditData),
                    "ReceiveWebRequest", new
                    {
                        identifier
                    }));
            }
        }


        protected override Task<bool> IsTriggered(ITrigger trigger, RecipeTrigger recipeTrigger,
            ReceiveWebRequestTriggerData triggerData,
            ReceiveWebRequestTriggerParameters parameters)
        {
            if (triggerData.Method != parameters.Method)
            {
                return Task.FromResult(false);
            }

            if (triggerData.RelativeUrl.Equals(parameters.RelativeUrl, StringComparison.InvariantCultureIgnoreCase))
            {
                return Task.FromResult(false);
            }

            switch (parameters.BodyComparer)
            {
                case ReceiveWebRequestTriggerParameters.FieldComparer.None:
                    return Task.FromResult(true);
                case ReceiveWebRequestTriggerParameters.FieldComparer.Equals:
                    return Task.FromResult(triggerData.Body.Equals(parameters.Body));
                case ReceiveWebRequestTriggerParameters.FieldComparer.Contains:
                    return Task.FromResult(triggerData.Body.Contains(parameters.Body));
                default:
                    return Task.FromResult(true);
            }
        }
    }
}