using System.Threading.Tasks;

namespace BtcTransmuter.Abstractions.Triggers
{
    public interface ITriggerDispatcher
    {
        Task DispatchTrigger(ITrigger trigger);
    }
}