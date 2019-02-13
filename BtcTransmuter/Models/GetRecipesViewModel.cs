using System.Collections.Generic;
using System.Linq;
using BtcTransmuter.Abstractions.Actions;
using BtcTransmuter.Abstractions.ExternalServices;
using BtcTransmuter.Abstractions.Triggers;
using BtcTransmuter.Data.Entities;

namespace BtcTransmuter.Controllers
{
    public class GetRecipesViewModel
    {
        public string StatusMessage { get; set; }
        public IEnumerable<Recipe> Recipes { get; set; }
        public IEnumerable<ITriggerDescriptor> TriggerDescriptors { get; set; }
        public IEnumerable<IExternalServiceDescriptor> ExternalServiceDescriptors { get; set; }
        public IEnumerable<IActionDescriptor> ActionDescriptors { get; set; }


        public ITriggerDescriptor GetDescriptorForTrigger(string triggerId)
        {
            return TriggerDescriptors
                .SingleOrDefault(descriptor => descriptor.TriggerId == triggerId);
        }

        public IActionDescriptor GetDescriptorForAction(string actionId)
        {
            return ActionDescriptors
                .SingleOrDefault(descriptor => descriptor.ActionId == actionId);
        }


        public IExternalServiceDescriptor GetDescriptorForExternalService(string externalServiceType)
        {
            return  ExternalServiceDescriptors
                .SingleOrDefault(descriptor => descriptor.ExternalServiceType == externalServiceType);
        }
    }
}