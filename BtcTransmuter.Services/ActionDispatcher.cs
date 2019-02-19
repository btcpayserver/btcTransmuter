using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BtcTransmuter.Abstractions.Actions;
using BtcTransmuter.Abstractions.Recipes;
using BtcTransmuter.Data.Entities;
using Newtonsoft.Json.Linq;

namespace BtcTransmuter.Services
{
    public class ActionDispatcher : IActionDispatcher
    {
        private readonly IEnumerable<IActionHandler> _handlers;
        private readonly IRecipeManager _recipeManager;

        public ActionDispatcher(IEnumerable<IActionHandler> handlers, IRecipeManager recipeManager)
        {
            _handlers = handlers;
            _recipeManager = recipeManager;
        }

        public async Task Dispatch(object triggerData, RecipeAction recipeAction)
        {
            foreach (var actionHandler in _handlers)
            {
                try
                {
                    var result = await actionHandler.Execute(triggerData, recipeAction);
                    if (result.Executed)
                    {
                        await _recipeManager.AddRecipeInvocation(new RecipeInvocation()
                        {
                            RecipeId = recipeAction.RecipeId,
                            Timestamp = DateTime.Now,
                            RecipeActionId = recipeAction.Id,
                            ActionResult = result.Result,
                            TriggerDataJson = JObject.FromObject(triggerData).ToString()
                        });
                    }
                }
                catch (Exception e)
                {
                    await _recipeManager.AddRecipeInvocation(new RecipeInvocation()
                    {
                        RecipeId = recipeAction.RecipeId,
                        Timestamp = DateTime.Now,
                        RecipeActionId = recipeAction.Id,
                        ActionResult = e.Message,
                        TriggerDataJson = JObject.FromObject(triggerData).ToString()
                    });
                }
                
            }
        }
    }
}