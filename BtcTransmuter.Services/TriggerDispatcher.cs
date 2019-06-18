using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BtcTransmuter.Abstractions.Actions;
using BtcTransmuter.Abstractions.Recipes;
using BtcTransmuter.Abstractions.Triggers;
using BtcTransmuter.Data.Entities;
using Microsoft.Extensions.Logging;

namespace BtcTransmuter.Services
{
    public class TriggerDispatcher : ITriggerDispatcher
    {
        private readonly IEnumerable<ITriggerHandler> _handlers;
        private readonly IRecipeManager _recipeManager;
        private readonly IActionDispatcher _actionDispatcher;
        private readonly ILogger<TriggerDispatcher> _logger;

        public TriggerDispatcher(IEnumerable<ITriggerHandler> handlers, IRecipeManager recipeManager,
            IActionDispatcher actionDispatcher, ILogger<TriggerDispatcher> logger)
        {
            _handlers = handlers;
            _recipeManager = recipeManager;
            _actionDispatcher = actionDispatcher;
            _logger = logger;
        }

        public async Task DispatchTrigger(ITrigger trigger)
        {
            _logger.LogInformation($"Trigger being dispatched: {trigger.Id}");
            var recipes = await _recipeManager.GetRecipes(new RecipesQuery()
            {
                Enabled = true,
                TriggerId = trigger.Id
            });

            _logger.LogInformation($"{recipes.Count()} possible recipes to be triggered by {trigger.Id}");

            var triggeredRecipes = new List<(Recipe Recipe, object TriggerData, ITriggerHandler triggerHandler)>();
            foreach (var recipe in recipes)
            {
                foreach (var triggerHandler in _handlers)
                {
                    try
                    {
                        if (await triggerHandler.IsTriggered(trigger, recipe.RecipeTrigger))
                        {
                            triggeredRecipes.Add((recipe, await triggerHandler.GetData(trigger), triggerHandler));
                        }
                    }
                    catch (Exception e)
                    {
                        _logger.LogError($"Trigger Handler {triggerHandler} errored out on {e.Message}");
                    }
                }
            }

            _logger.LogInformation($"{trigger.Id} triggered {triggeredRecipes.Count()}/{recipes.Count()} recipes");

            var nonGroupedRecipeTasks = triggeredRecipes.SelectMany(recipe =>
                recipe.Recipe.RecipeActions
                    .Where(recipeAction => string.IsNullOrEmpty(recipeAction.RecipeActionGroupId))
                    .Select(action => (Task) _actionDispatcher.Dispatch(
                        new Dictionary<string, (object data, string json)>()
                        {
                            {"TriggerData", (recipe.TriggerData, trigger.DataJson)}
                        }, action)));

            var groupExecutionTasks = triggeredRecipes.SelectMany(recipe => recipe.Recipe.RecipeActionGroups.Select(
                actionGroup => _actionDispatcher.Dispatch(new Dictionary<string, (object data, string json)>()
                {
                    {"TriggerData", (recipe.TriggerData, trigger.DataJson)}
                }, actionGroup)));

            await Task.WhenAll(nonGroupedRecipeTasks.Concat(groupExecutionTasks));

            foreach (var keyValuePair in triggeredRecipes.GroupBy(tuple => tuple.triggerHandler)
                .ToDictionary(tuples => tuples.Key, tuples => tuples.ToArray()))
            {
                await keyValuePair.Key.AfterExecution(keyValuePair.Value.Select(tuple => tuple.Recipe));
            }
        }
    }
}