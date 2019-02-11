using System.Threading.Tasks;
using BtcTransmuter.Data;

namespace BtcTransmuter.Abstractions.Triggers
{
    public interface ITriggerHandler
    {
        Task<bool> IsTriggered(ITrigger trigger, RecipeTrigger recipeTrigger);

        Task<object> GetData(ITrigger trigger);
    }
}