using System.Threading.Tasks;
using BtcTransmuter.Data;

namespace BtcTransmuter.Abstractions.Actions
{
    public interface IActionDispatcher
    {
        Task Dispatch(object triggerData, RecipeAction recipeAction);
    }
}