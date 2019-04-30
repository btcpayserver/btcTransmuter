using System;
using System.Threading.Tasks;
using BtcTransmuter.Abstractions.Triggers;
using BtcTransmuter.Data.Entities;

namespace BtcTransmuter.Extension.Webhook.Triggers.DynamicServiceMarker
{
    public class DynamicServiceMarkerTriggerHandler : BaseTriggerHandler<DynamicServiceMarkerTriggerData,
        DynamicServiceMarkerTriggerParameters>
    {
        public override string TriggerId => new DynamicServiceMarkerTrigger().Id;
        public override string Name => "Dynamic Service Marker";

        public override string Description =>
            "Used to mark a recipe to be sued for a Dynamic External Service";

        public override string ViewPartial => "ViewDynamicServiceMarkerTrigger";
        public override string ControllerName => "DynamicServiceMarker";


        protected override Task<bool> IsTriggered(ITrigger trigger, RecipeTrigger recipeTrigger,
            DynamicServiceMarkerTriggerData triggerData,
            DynamicServiceMarkerTriggerParameters parameters)
        {
            return Task.FromResult(false);
        }
    }
}