using System.Collections;
using System.Collections.Generic;
using BtcTransmuter.Abstractions.Actions;
using BtcTransmuter.Abstractions.ExternalServices;
using BtcTransmuter.Abstractions.Triggers;
using ExtCore.Infrastructure;

namespace BtcTransmuter.Models
{
    public class ExtensionsViewModel
    {
        public IEnumerable<IExtension>  Extensions{ get; set; }

        public IEnumerable<IActionDescriptor> ActionDescriptors { get; set; }
        public IEnumerable<ITriggerDescriptor> TriggerDescriptors { get; set; }
        public IEnumerable<IExternalServiceDescriptor> ExternalServiceDescriptors { get; set; }
    }
}