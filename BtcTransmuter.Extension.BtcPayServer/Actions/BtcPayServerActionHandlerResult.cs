using BtcTransmuter.Abstractions.Actions;

namespace BtcTransmuter.Extension.BtcPayServer.Actions
{
    public class BtcPayServerActionHandlerResult<T> : TypedActionHandlerResult<T>
    {
        public override string DataJson => NBitcoin.JsonConverters.Serializer.ToString(Data);
    }
}