using BtcTransmuter.Abstractions.Triggers;
using NBXplorer;
using NBXplorer.DerivationStrategy;

namespace BtcTransmuter.Extension.NBXplorer.Triggers.NBXplorerNewTransaction
{
    public class NBXplorerNewTransactionTrigger : BaseTrigger<NBXplorerNewTransactionTriggerData>
    {
        public new static readonly string Id = typeof(NBXplorerNewTransactionTrigger).FullName;
        private readonly ExplorerClient _explorerClient;

        public NBXplorerNewTransactionTrigger(ExplorerClient explorerClient)
        {
            _explorerClient = explorerClient;
        }

        public override NBXplorerNewTransactionTriggerData Data
        {
            get => base.Data;
            set => DataJson = _explorerClient.Serializer.ToString(value);
        }
    }
}