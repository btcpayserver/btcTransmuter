using BtcTransmuter.Abstractions.Triggers;
using NBXplorer;

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
            get => _explorerClient.Serializer.ToObject<NBXplorerNewTransactionTriggerData>(DataJson);
            set => DataJson = _explorerClient.Serializer.ToString(value);
        }
    }
}