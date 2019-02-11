using System.Threading.Tasks;
using BtcTransmuter.Abstractions.ExternalServices;
using BtcTransmuter.Data;
using BtcTransmuter.Data.Models;

namespace BtcTransmuter.Abstractions.Triggers
{
    public abstract class BaseTriggerHandler<TTriggerData, TTriggerParameters> : ITriggerHandler where TTriggerParameters : class
    {
        protected abstract Task<bool> IsTriggered(ITrigger trigger, RecipeTrigger recipeTrigger, TTriggerData triggerData ,TTriggerParameters parameters);

        public Task<bool> IsTriggered(ITrigger trigger, RecipeTrigger recipeTrigger)
        {
            if (recipeTrigger.TriggerId != trigger.Id)
            {
                return Task.FromResult(false);
            }

            var triggerData = trigger.Get<TTriggerData>();
            
            if (typeof(TTriggerParameters).IsAssignableFrom(typeof(IUseExternalService)) && 
                ((IUseExternalService) triggerData).ExternalServiceId != recipeTrigger.ExternalServiceId)
            {
                return Task.FromResult(false);
            }
            return IsTriggered(trigger, recipeTrigger,triggerData , recipeTrigger.Get<TTriggerParameters>());
        }

        public Task<object> GetData(ITrigger trigger)
        {
            return Task.FromResult((object) trigger.Get<TTriggerData>());
        }
    }
}