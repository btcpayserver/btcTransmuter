using System.Threading.Tasks;

namespace BtcTransmuter.Abstractions
{
    public interface ITriggerDispatcher
    {
        Task DispatchTrigger<T>(ITrigger<T> trigger);
    }
}