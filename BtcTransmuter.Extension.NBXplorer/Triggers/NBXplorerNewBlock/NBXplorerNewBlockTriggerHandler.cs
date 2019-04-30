using System;
using System.Threading.Tasks;
using BtcTransmuter.Abstractions.Triggers;
using BtcTransmuter.Data.Entities;

namespace BtcTransmuter.Extension.NBXplorer.Triggers.NBXplorerNewBlock
{
    public class NBXplorerNewBlockTriggerHandler : BaseTriggerHandler<NBXplorerNewBlockTriggerData,
        NBXplorerNewBlockTriggerParameters>
    {
        public override string TriggerId => new NBXplorerNewBlockTrigger().Id;
        public override string Name => "New Block";

        public override string Description =>
            "Trigger a recipe by detecting new blocks";

        public override string ViewPartial => "ViewNBXplorerNewBlockTrigger";
        public override string ControllerName => "NBXplorerNewBlock";

        protected override Task<bool> IsTriggered(ITrigger trigger, RecipeTrigger recipeTrigger,
            NBXplorerNewBlockTriggerData triggerData,
            NBXplorerNewBlockTriggerParameters parameters)
        {
            return Task.FromResult(!triggerData.CryptoCode.Equals(parameters.CryptoCode,
                StringComparison.InvariantCultureIgnoreCase));
        }
    }
}