using System.Threading.Tasks;
using BtcTransmuter.Data;

namespace BtcTransmuter.Abstractions.Actions
{
    public interface IActionHandler
    {
        Task<bool> Execute(object triggerData, RecipeAction recipeAction);
    }
}