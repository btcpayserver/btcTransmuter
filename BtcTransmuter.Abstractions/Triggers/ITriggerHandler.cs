using System.Threading.Tasks;
using BtcTransmuter.Data;

namespace BtcTransmuter.Abstractions
{
    public interface ITriggerHandler
    {
        Task<bool> IsTriggered(ITrigger trigger, RecipeTrigger recipeTrigger);
    }
}