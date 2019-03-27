using BtcTransmuter.Abstractions.Triggers;
using NBXplorer;

namespace BtcTransmuter.Extension.NBXplorer.Triggers.NBXplorerBalance
{
    public class NBXplorerBalanceTrigger : BaseTrigger<NBXplorerBalanceTriggerData>
    {
        public new static readonly string Id = typeof(NBXplorerBalanceTrigger).FullName;
        public readonly ExplorerClient _explorerClient;

        public NBXplorerBalanceTrigger(ExplorerClient explorerClient)
        {
            _explorerClient = explorerClient;
        }

        public override NBXplorerBalanceTriggerData Data
        {
            get => _explorerClient.Serializer.ToObject<NBXplorerBalanceTriggerData>(DataJson);
            set => DataJson = _explorerClient.Serializer.ToString(value);
        }
    }
}