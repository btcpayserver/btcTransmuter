using System.Threading.Tasks;
using BtcTransmuter.Data;
using BtcTransmuter.Data.Models;

namespace BtcTransmuter.Abstractions
{
    public abstract class BaseTriggerHandler<TTriggerData, TTriggerParameters> : ITriggerHandler where TTriggerParameters : class
    {
        protected abstract Task<bool> IsTriggered(ITrigger trigger, RecipeTrigger recipeTrigger, TTriggerData triggerData ,TTriggerParameters parameters);

        public Task<bool> IsTriggered(ITrigger trigger, RecipeTrigger recipeTrigger)
        {
            return IsTriggered(trigger, recipeTrigger, trigger.Get<TTriggerData>() , recipeTrigger.Get<TTriggerParameters>());
        }
    }

}