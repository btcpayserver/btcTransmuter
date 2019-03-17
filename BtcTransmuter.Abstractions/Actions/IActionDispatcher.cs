using System.Collections.Generic;
using System.Threading.Tasks;
using BtcTransmuter.Data;
using BtcTransmuter.Data.Entities;

namespace BtcTransmuter.Abstractions.Actions
{
    public interface IActionDispatcher
    {
        Task<IEnumerable<ActionHandlerResult>> Dispatch(object triggerData, RecipeAction recipeAction);
        Task Dispatch(object triggerData, RecipeActionGroup recipeActionGroup);
    }
}