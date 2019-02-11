using System.Collections.Generic;
using BtcTransmuter.Abstractions.Actions;
using BtcTransmuter.Abstractions.ExternalServices;
using BtcTransmuter.Abstractions.Triggers;
using BtcTransmuter.Models;
using ExtCore.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BtcTransmuter.Controllers
{
//    [Authorize(Roles = "Admin")]
    public class AdminController:Controller
    {

        private readonly IEnumerable<IExtension> _extensions;
        private readonly IEnumerable<IExternalServiceDescriptor> _externalServiceDescriptors;
        private readonly IEnumerable<IActionDescriptor> _actionDescriptors;
        private readonly IEnumerable<ITriggerDescriptor> _triggerDescriptors;

        public AdminController(IEnumerable<IExtension> extensions, 
            IEnumerable<IExternalServiceDescriptor> externalServiceDescriptors, 
            IEnumerable<IActionDescriptor> actionDescriptors, 
            IEnumerable<ITriggerDescriptor> triggerDescriptors)
        {
            _extensions = extensions;
            _externalServiceDescriptors = externalServiceDescriptors;
            _actionDescriptors = actionDescriptors;
            _triggerDescriptors = triggerDescriptors;
        }
        public IActionResult Extensions()
        {
            return View(new ExtensionsViewModel()
            {
                Extensions = _extensions,
                ActionDescriptors = _actionDescriptors,
                TriggerDescriptors = _triggerDescriptors,
                ExternalServiceDescriptors =  _externalServiceDescriptors
            });
        }
    }
}