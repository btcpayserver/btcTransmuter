using System;
using System.Collections.Generic;
using System.Linq;
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

        public async Task<IEnumerable<ActionHandlerResult>> Dispatch(
            Dictionary<string, (object data, string json)> additionalData,
            RecipeAction recipeAction)
        {
            _logger.LogInformation($"Dispatching {recipeAction.ActionId} for recipe {recipeAction.RecipeId}");
            var result = new List<ActionHandlerResult>();
            foreach (var actionHandler in _handlers)
            {
                try
                {
                    var actionHandlerResult = await actionHandler.Execute(additionalData, recipeAction);
                    result.Add(actionHandlerResult);
                    if (actionHandlerResult.Executed)
                    {
                        _logger.LogInformation(
                            $"{recipeAction.ActionId} for recipe {recipeAction.RecipeId} was executed");

                        await _recipeManager.AddRecipeInvocation(new RecipeInvocation()
                        {
                            RecipeId = recipeAction.RecipeId,
                            Timestamp = DateTime.Now,
                            RecipeActionId = recipeAction.Id,
                            ActionResult = actionHandlerResult.Result,
                            TriggerDataJson = JObject
                                .FromObject(additionalData.ToDictionary(pair => pair.Key, pair => pair.Value.json))
                                .ToString()
                        });
                        continue;
                    }

                    _logger.LogInformation(
                        $"{recipeAction.ActionId} for recipe {recipeAction.RecipeId} was not executed because [{actionHandlerResult.Result}]");
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
                        TriggerDataJson = JObject.FromObject(additionalData).ToString()
                    });
                }
            }

            return result;
        }

        public async Task Dispatch(Dictionary<string, (object data, string json)> data,
            RecipeActionGroup recipeActionGroup)
        {
            _logger.LogInformation(
                $"Dispatching action group {recipeActionGroup.Id} for recipe {recipeActionGroup.RecipeId}");
            await RecursiveActionExecution(
                new Queue<RecipeAction>(recipeActionGroup.RecipeActions.OrderBy(action => action.Order)), data);
        }

        private async Task RecursiveActionExecution(Queue<RecipeAction> recipeActions,
            Dictionary<string, (object data, string json)> data)
        {
            var action = recipeActions.Dequeue();
            var result = await Dispatch(data, action);
            var continuablePaths = result.Where(i => i.Executed);
            foreach (var path in continuablePaths)
            {
                if (data.ContainsKey("PreviousAction"))
                {
                    data.Remove("PreviousAction");
                }

                data.Add("PreviousAction", (path.Data, JObject.FromObject(path.Data).ToString()));

                await RecursiveActionExecution(recipeActions, data);
            }
        }
    }
}