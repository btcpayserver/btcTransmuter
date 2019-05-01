using BtcTransmuter.Extension.NBXplorer.Triggers.NBXplorerNewBlock;
using BtcTransmuter.Tests.Base;

namespace BtcTransmuter.Extension.NBXplorer.Tests
{
    public class NBXplorerNewBlockTriggerHandlerTests : BaseTriggerTest<NBXplorerNewBlockTriggerHandler,
        NBXplorerNewBlockTriggerData,
        NBXplorerNewBlockTriggerParameters>
    {
        protected override NBXplorerNewBlockTriggerHandler GetTriggerHandlerInstance(params object[] setupArgs)
        {
            return new NBXplorerNewBlockTriggerHandler();
        }
    }
}