using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BtcTransmuter.Abstractions.Actions;
using BtcTransmuter.Abstractions.Recipes;
using BtcTransmuter.Data.Entities;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace BtcTransmuter.Services
{
    public class ActionDispatcher : IActionDispatcher
    {
        private readonly IEnumerable<IActionHandler> _handlers;
        private readonly IRecipeManager _recipeManager;
        private readonly ILogger<ActionDispatcher> _logger;

        public ActionDispatcher(IEnumerable<IActionHandler> handlers, IRecipeManager recipeManager,
            ILogger<ActionDispatcher> logger)
        {
            _handlers = handlers;
            _recipeManager = recipeManager;
            _logger = logger;
        }

        public async Task Dispatch(object triggerData, RecipeAction recipeAction)
        {
            _logger.LogInformation($"Dispatching {recipeAction.ActionId} for recipe {recipeAction.RecipeId}");
            foreach (var actionHandler in _handlers)
            {
                try
                {
                    var result = await actionHandler.Execute(triggerData, recipeAction);
                    if (result.Executed)
                    {
                        _logger.LogInformation(
                            $"{recipeAction.ActionId} for recipe {recipeAction.RecipeId} was executed");

                        await _recipeManager.AddRecipeInvocation(new RecipeInvocation()
                        {
                            RecipeId = recipeAction.RecipeId,
                            Timestamp = DateTime.Now,
                            RecipeActionId = recipeAction.Id,
                            ActionResult = result.Result,
                            TriggerDataJson = JObject.FromObject(triggerData).ToString()
                        });
                        continue;
                    }

                    _logger.LogInformation(
                        $"{recipeAction.ActionId} for recipe {recipeAction.RecipeId} was not executed because [{result.Result}]");
                }
                catch (Exception e)
                {
                    _logger.LogInformation(
                        $"{recipeAction.ActionId} for recipe {recipeAction.RecipeId} failed with error: {e.Message}");
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