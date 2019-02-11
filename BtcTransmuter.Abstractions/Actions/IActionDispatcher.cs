using System.Threading.Tasks;
using BtcTransmuter.Data;
using BtcTransmuter.Data.Entities;

namespace BtcTransmuter.Abstractions.Actions
{
    public interface IActionDispatcher
    {
        Task Dispatch(object triggerData, RecipeAction recipeAction);
    }
}