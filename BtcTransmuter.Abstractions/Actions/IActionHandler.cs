using System.Threading.Tasks;
using BtcTransmuter.Data;
using BtcTransmuter.Data.Entities;

namespace BtcTransmuter.Abstractions.Actions
{
    public interface IActionHandler
    {
        Task<bool> Execute(object triggerData, RecipeAction recipeAction);
    }
}