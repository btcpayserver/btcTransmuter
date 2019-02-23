using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BtcTransmuter.Abstractions.Actions;
using BtcTransmuter.Abstractions.Recipes;
using BtcTransmuter.Abstractions.Triggers;
using BtcTransmuter.Data;
using BtcTransmuter.Data.Entities;

namespace BtcTransmuter.Services
{
    public class TriggerDispatcher : ITriggerDispatcher
    {
        private readonly IEnumerable<ITriggerHandler> _handlers;
        private readonly IRecipeManager _recipeManager;
        private readonly IActionDispatcher _actionDispatcher;

        public TriggerDispatcher(IEnumerable<ITriggerHandler> handlers, IRecipeManager recipeManager,
            IActionDispatcher actionDispatcher)
        {
            _handlers = handlers;
            _recipeManager = recipeManager;
            _actionDispatcher = actionDispatcher;
        }

        public async Task DispatchTrigger(ITrigger trigger)
        {
            var recipes = await _recipeManager.GetRecipes(new RecipesQuery()
            {
                Enabled = true,
                RecipeTriggerId = trigger.Id
            });


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
                    catch
                    {
                    }
                }
            }

            await Task.WhenAll(triggeredRecipes.SelectMany(recipe =>
                recipe.Recipe.RecipeActions.Select(action => _actionDispatcher.Dispatch(recipe.TriggerData, action))));

            foreach (var keyValuePair in triggeredRecipes.GroupBy(tuple => tuple.triggerHandler)
                .ToDictionary(tuples => tuples.Key, tuples => tuples.ToArray()))
            {
               await  keyValuePair.Key.AfterExecution(keyValuePair.Value.Select(tuple => tuple.Recipe));
            }
            
            
        }
    }
}