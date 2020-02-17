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
        Task<IEnumerable<RecipeInvocation>> GetRecipeInvocations(RecipeInvocationsQuery query);
        
        Task AddOrUpdateRecipe(Recipe recipe);
        Task AddOrUpdateRecipeTrigger(RecipeTrigger trigger);
        Task AddOrUpdateRecipeTriggers(IEnumerable<RecipeTrigger> triggers);
        Task AddOrUpdateRecipeAction(RecipeAction action);
        Task AddRecipeActionGroup(RecipeActionGroup recipeActionGroup);
        Task ReorderRecipeActionGroupActions(string recipeActionGroupId, Dictionary<string, int> actionsOrder);
        Task RemoveRecipe(string id);
        Task<Recipe> GetRecipe(string id, string userId = null);
        Task AddRecipeInvocation(RecipeInvocation invocation);
        Task RemoveRecipeAction(string recipeActionId);
        Task RemoveRecipeTrigger(string recipeTriggerId);
        Task RemoveRecipeActionGroup(string recipeActionGroupId);
        Task<string> GetRecipeName(string recipeId);
        Task<Recipe> CloneRecipe(string recipeId, bool enable, string newName = null);

    }
}