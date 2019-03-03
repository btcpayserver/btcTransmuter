using System;
using System.Threading.Tasks;
using BtcTransmuter.Abstractions.Triggers;
using BtcTransmuter.Data.Entities;

namespace BtcTransmuter.Extension.NBXplorer.Triggers.NBXplorerNewTransaction
{
    public class NBXplorerNewTransactionTriggerHandler : BaseTriggerHandler<NBXplorerNewTransactionTriggerData,
        NBXplorerNewTransactionTriggerParameters>
    {
        public override string TriggerId => new NBXplorerNewTransactionTrigger().Id;
        public override string Name => "New Block";

        public override string Description =>
            "Trigger a recipe by detecting new blocks";

        public override string ViewPartial => "ViewNBXplorerNewTransactionTrigger";
        protected override string ControllerName => "NBXplorerNewTransaction";

        protected override Task<bool> IsTriggered(ITrigger trigger, RecipeTrigger recipeTrigger,
            NBXplorerNewTransactionTriggerData triggerData,
            NBXplorerNewTransactionTriggerParameters parameters)
        {
            return Task.FromResult(string.IsNullOrEmpty(parameters.CryptoCode) ||
                                   !triggerData.CryptoCode.Equals(parameters.CryptoCode,
                                       StringComparison.InvariantCultureIgnoreCase));
        }
    }
}