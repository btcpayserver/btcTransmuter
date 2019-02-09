using System.Threading.Tasks;

namespace BtcTransmuter.Abstractions
{
    public interface ITriggerHandler<TTriggerData>
    {
        Task<bool> IsTriggered(ITrigger<TTriggerData> trigger, object parameters);
    }
}