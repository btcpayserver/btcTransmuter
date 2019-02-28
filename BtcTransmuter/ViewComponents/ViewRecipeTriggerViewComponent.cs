using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BtcTransmuter.Abstractions.Triggers;
using BtcTransmuter.Data.Entities;
using BtcTransmuter.Models;
using Microsoft.AspNetCore.Mvc;

namespace BtcTransmuter.ViewComponents
{
    public class ViewRecipeTriggerViewComponent : ViewComponent
    {
        private readonly IEnumerable<ITriggerDescriptor> _triggerDescriptors;

        public ViewRecipeTriggerViewComponent(IEnumerable<ITriggerDescriptor> triggerDescriptors)
        {
            _triggerDescriptors = triggerDescriptors;
        }

        public Task<IViewComponentResult> InvokeAsync(RecipeTrigger recipeTrigger)
        {
            return Task.FromResult<IViewComponentResult>(View(new ViewRecipeTriggerViewModel()
            {
                RecipeTrigger = recipeTrigger,
                ExternalServiceData = recipeTrigger.ExternalService,
                TriggerDescriptor =
                    _triggerDescriptors.SingleOrDefault(descriptor => descriptor.TriggerId == recipeTrigger.TriggerId)
            }));
        }
    }
}