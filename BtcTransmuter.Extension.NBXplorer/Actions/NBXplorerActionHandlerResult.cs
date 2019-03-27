using BtcTransmuter.Abstractions.Actions;
using NBXplorer;

namespace BtcTransmuter.Extension.NBXplorer.Actions.GenerateNextAddress
{
    public class NBXplorerActionHandlerResult<T> : TypedActionHandlerResult<T>
    {
        private readonly ExplorerClient _explorerClient;

        public NBXplorerActionHandlerResult(ExplorerClient explorerClient)
        {
            _explorerClient = explorerClient;
        }
        public override string DataJson => _explorerClient.Serializer.ToString(Data);
    }
}