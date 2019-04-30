using BtcTransmuter.Extension.Webhook.Triggers.DynamicServiceMarker;
using BtcTransmuter.Tests.Base;

namespace BtcTransmuter.Extension.DynamicServices.Tests
{
    public class DynamicServiceMarkerTriggerHandlerTests : BaseTriggerTest<DynamicServiceMarkerTriggerHandler,
        DynamicServiceMarkerTriggerData,
        DynamicServiceMarkerTriggerParameters>
    {
        protected override DynamicServiceMarkerTriggerHandler GetTriggerHandlerInstance(params object[] setupArgs)
        {
            return new DynamicServiceMarkerTriggerHandler();
        }
    }
}