using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using BtcTransmuter.Data;

namespace BtcTransmuter.Abstractions.Recipes
{
    public interface IRecipeManager
    {
        Task<IEnumerable<Recipe>> GetRecipes(RecipesQuery query);
    }


    public class RecipesQuery
    {
        public bool Enabled { get; set; }
        public string RecipeTriggerId { get; set; }
    }
}