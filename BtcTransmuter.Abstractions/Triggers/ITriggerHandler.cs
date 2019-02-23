using System.Collections.Generic;
using System.Threading.Tasks;
using BtcTransmuter.Data;
using BtcTransmuter.Data.Entities;

namespace BtcTransmuter.Abstractions.Triggers
{
    public interface ITriggerHandler
    {
        Task<bool> IsTriggered(ITrigger trigger, RecipeTrigger recipeTrigger);

        Task<object> GetData(ITrigger trigger);

        Task AfterExecution(IEnumerable<Recipe> recipes);
    }
}