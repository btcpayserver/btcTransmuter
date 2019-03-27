using System.Collections.Generic;
using System.Threading.Tasks;
using BtcTransmuter.Data;
using BtcTransmuter.Data.Entities;

namespace BtcTransmuter.Abstractions.Actions
{
    public interface IActionDispatcher
    {
        Task<IEnumerable<ActionHandlerResult>> Dispatch(Dictionary<string, (object data, string json)> triggerData,
            RecipeAction recipeAction);

        Task Dispatch(Dictionary<string, (object data, string json)> data, RecipeActionGroup recipeActionGroup);
    }
}