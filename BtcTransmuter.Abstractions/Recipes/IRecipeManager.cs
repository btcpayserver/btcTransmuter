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
        Task RemoveRecipe(string id);
        Task<Recipe> GetRecipe (string id);
        
    }
}