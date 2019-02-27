using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using BtcTransmuter.Data;
using BtcTransmuter.Data.Entities;

namespace BtcTransmuter.Abstractions.Recipes
{
    public interface IRecipeManager
    {
        Task<IEnumerable<Recipe>> GetRecipes(RecipesQuery query);
        Task AddOrUpdateRecipe(Recipe recipe);
        Task AddOrUpdateRecipeTrigger(RecipeTrigger trigger);
        Task AddOrUpdateRecipeTriggers(IEnumerable<RecipeTrigger> triggers);
        Task AddOrUpdateRecipeAction(RecipeAction action);
        Task RemoveRecipe(string id);
        Task<Recipe> GetRecipe(string id, string userId = null);
        Task AddRecipeInvocation(RecipeInvocation invocation);
        Task RemoveRecipeAction(string recipeActionId);
        Task RemoveRecipeTrigger(string recipeTriggerId);
        Task<string> GetRecipeName(string recipeId);
    }
}