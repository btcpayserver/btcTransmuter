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
            var dataDict = additionalData.ToDictionary(pair => pair.Key, pair => pair.Value.json);
            var validHandlersTasks =
                _handlers.Select(handler => (handler, handler.CanExecute(additionalData, recipeAction))).ToList();
            await Task.WhenAll(validHandlersTasks.Select(tuple => tuple.Item2));
            var validHandlers = validHandlersTasks.Where(tuple => tuple.Item2.Result).Select(tuple => tuple.Item1);
            foreach (var actionHandler in validHandlers)
            {
                try
                {
                    var actionHandlerResult = await actionHandler.Execute(additionalData, recipeAction);
                    result.Add(actionHandlerResult);

                    await _recipeManager.AddRecipeInvocation(new RecipeInvocation()
                    {
                        RecipeId = recipeAction.RecipeId,
                        Timestamp = DateTime.Now,
                        RecipeAction = recipeAction.ToString(),
                        ActionResult = actionHandlerResult.Result,
                        TriggerDataJson = JObject
                            .FromObject(dataDict)
                            .ToString()
                    });
                    if (actionHandlerResult.Executed)
                    {
                        _logger.LogInformation(
                            $"{recipeAction.ActionId} for recipe {recipeAction.RecipeId} was executed");

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
                        RecipeAction = recipeAction.ToString(),
                        ActionResult = e.Message,
                        TriggerDataJson = JObject
                            .FromObject(additionalData.ToDictionary(pair => pair.Key, pair => pair.Value.json))
                            .ToString()
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
            if (!recipeActions.Any())
            {
                return;
            }
            var action = recipeActions.Dequeue();
            var result = await Dispatch(data, action);
            var continuablePaths = result.Where(i => i.Executed);
            foreach (var path in continuablePaths)
            {
                if (data.ContainsKey("PreviousAction"))
                {
                    data.Remove("PreviousAction");
                }

                var depth = data.Keys
                        
                    .Where(s => s.StartsWith("ActionData", StringComparison.InvariantCultureIgnoreCase))
                    .Select(s => int.Parse(s.Replace("ActionData", ""))).DefaultIfEmpty(-1).Max();
                data.Add("PreviousAction", (path.Data, path.DataJson));
                data.Add($"ActionData{depth + 1}", (path.Data, path.DataJson));

                await RecursiveActionExecution(recipeActions, data);
            }
        }
    }
}