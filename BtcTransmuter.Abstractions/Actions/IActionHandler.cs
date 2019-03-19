using System.Collections.Generic;
using System.Threading.Tasks;
using BtcTransmuter.Data;
using BtcTransmuter.Data.Entities;

namespace BtcTransmuter.Abstractions.Actions
{
    public interface IActionHandler
    {
        Task<ActionHandlerResult> Execute(Dictionary<string, object> data, RecipeAction recipeAction);
    }
}