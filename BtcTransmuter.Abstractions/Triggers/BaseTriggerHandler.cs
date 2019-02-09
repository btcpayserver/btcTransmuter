using System.Threading.Tasks;

namespace BtcTransmuter.Abstractions
{
    public abstract class BaseTriggerHandler<TTrigger, TTriggerData, TTriggerParameters> : ITriggerHandler<TTriggerData>
        where TTrigger : ITrigger<TTriggerData> where TTriggerParameters : class
    {
        public async Task<bool> IsTriggered(ITrigger<TTriggerData> trigger, object parameters)
        {
            if (!(parameters is TTriggerParameters))
            {
                return false;
            }

            return await IsTriggered(trigger, (TTriggerParameters) parameters);
        }

        protected abstract Task<bool> IsTriggered(ITrigger<TTriggerData> trigger, TTriggerParameters parameters);
    }
}