using BtcTransmuter.Abstractions.ExternalServices;
using BtcTransmuter.Abstractions.Triggers;
using BtcTransmuter.Data.Entities;

namespace BtcTransmuter.Models
{
    public class ViewRecipeTriggerViewModel
    {
        public ExternalServiceData ExternalServiceData { get; set; }
        public RecipeTrigger RecipeTrigger { get; set; }
        public ITriggerDescriptor TriggerDescriptor { get; set; }
    }
}