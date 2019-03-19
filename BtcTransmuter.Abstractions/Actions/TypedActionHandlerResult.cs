namespace BtcTransmuter.Abstractions.Actions
{
    public class TypedActionHandlerResult<T>: ActionHandlerResult
    {
        public T TypedData
        {
            get => (T) Data;
            set => Data = value;
        }
    }
}