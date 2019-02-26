using System.Threading.Tasks;
using BtcTransmuter.Abstractions.Actions;
using BtcTransmuter.Abstractions.Recipes;
using BtcTransmuter.Data.Entities;
using BtcTransmuter.Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Caching.Memory;

namespace BtcTransmuter.Extension.Recipe.Actions.ToggleRecipe
{
    [Route("recipe-plugin/actions/toggle-recipe")]
    [Authorize]
    public class
        ToggleRecipeController : BaseActionController<ToggleRecipeController.ToggleRecipeViewModel, ToggleRecipeData>
    {
        public ToggleRecipeController(IMemoryCache memoryCache, UserManager<User> userManager,
            IRecipeManager recipeManager) : base(memoryCache, userManager, recipeManager)
        {
        }

        public class ToggleRecipeViewModel : ToggleRecipeData
        {
            public SelectList Recipes { get; set; }
            public string RecipeId { get; set; }
        }

        protected override async Task<ToggleRecipeViewModel> BuildViewModel(RecipeAction from)
        {
            var data = from.Get<ToggleRecipeData>();
            return new ToggleRecipeViewModel
            {
                RecipeId = from.RecipeId,
                Recipes = new SelectList(
                    await _recipeManager.GetRecipes(new RecipesQuery() {UserId = _userManager.GetUserId(User)}),
                    nameof(BtcTransmuter.Data.Entities.Recipe.Id), nameof(BtcTransmuter.Data.Entities.Recipe.Name),
                    data.TargetRecipeId),
                TargetRecipeId = data.TargetRecipeId
            };
        }

        protected override async Task<ToggleRecipeViewModel> BuildViewModel(ToggleRecipeViewModel vm)
        {
            vm.Recipes = new SelectList(
                await _recipeManager.GetRecipes(new RecipesQuery() {UserId = _userManager.GetUserId(User)}),
                nameof(BtcTransmuter.Data.Entities.Recipe.Id), nameof(BtcTransmuter.Data.Entities.Recipe.Name),
                vm.TargetRecipeId);
            return vm;
        }
    }
}