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
        private readonly IEnumerable<IExternalServiceDescriptor> _externalServiceDescriptors;
        private readonly IEnumerable<BtcTransmuterExtension> _extensions;

        public ViewRecipeTriggerViewComponent(IEnumerable<ITriggerDescriptor> triggerDescriptors,
            IEnumerable<IExternalServiceDescriptor> externalServiceDescriptors)
        {
            triggerDescriptors = triggerDescriptors;
            _externalServiceDescriptors = externalServiceDescriptors;
        }

        public async Task<IViewComponentResult> InvokeAsync(RecipeTrigger recipeTrigger)
        {
            return View(new ViewRecipeTriggerViewModel()
            {
                RecipeTrigger = recipeTrigger,
                ExternalServiceData = recipeTrigger.ExternalService,
                TriggerDescriptor = 
                    _triggerDescriptors.Single(descriptor => descriptor.TriggerId == recipeTrigger.TriggerId),
                ExternalServiceDescriptor = _externalServiceDescriptors
                    .Single(descriptor => descriptor.ExternalServiceType == recipeTrigger.ExternalService.Type)
            });
        }
    }
}