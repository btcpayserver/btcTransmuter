using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BtcTransmuter.Abstractions.Extensions;
using BtcTransmuter.Abstractions.ExternalServices;
using BtcTransmuter.Abstractions.Triggers;
using BtcTransmuter.Data.Entities;
using BtcTransmuter.Models;
using Microsoft.AspNetCore.Mvc;

namespace BtcTransmuter.Areas.ViewComponents
{
    public class ViewRecipeTriggerViewComponent : ViewComponent
    {
        private readonly IEnumerable<ITriggerDescriptor> _triggerDescriptors;

        public ViewRecipeTriggerViewComponent(IEnumerable<ITriggerDescriptor> triggerDescriptors)
        {
            triggerDescriptors = triggerDescriptors;
        }

        public async Task<IViewComponentResult> InvokeAsync(RecipeTrigger recipeTrigger)
        {
            return View(new ViewRecipeTriggerViewModel()
            {
                RecipeTrigger = recipeTrigger,
                ExternalServiceData = recipeTrigger.ExternalService,
                TriggerDescriptor =
                    _triggerDescriptors.Single(descriptor => descriptor.TriggerId == recipeTrigger.TriggerId)
            });
        }
    }
}