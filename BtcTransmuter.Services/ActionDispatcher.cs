using System.Collections.Generic;
using System.Threading.Tasks;
using BtcTransmuter.Abstractions.Actions;
using BtcTransmuter.Data;
using BtcTransmuter.Data.Entities;

namespace BtcTransmuter.Services
{
    public class ActionDispatcher : IActionDispatcher
    {
        private readonly IEnumerable<IActionHandler> _handlers;

        public ActionDispatcher(IEnumerable<IActionHandler> handlers)
        {
            _handlers = handlers;
        }

        public async Task Dispatch(object triggerData, RecipeAction recipeAction)
        {
            foreach (var actionHandler in _handlers)
            {
                await actionHandler.Execute(triggerData, recipeAction);
            }
        }
    }
}