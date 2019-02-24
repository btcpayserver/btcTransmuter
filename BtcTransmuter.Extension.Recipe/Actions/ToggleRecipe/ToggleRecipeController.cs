using System.Threading.Tasks;
using BtcTransmuter.Abstractions.Recipes;
using BtcTransmuter.Data.Entities;
using BtcTransmuter.Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Caching.Memory;

namespace BtcTransmuter.Extension.Recipe.Actions.CreateRecipe
{
    [Route("recipe-plugin/actions/toggle-recipe")]
    [Authorize]
    public class ToggleRecipeController : Controller
    {
        private readonly IRecipeManager _recipeManager;
        private readonly UserManager<User> _userManager;
        private readonly IMemoryCache _memoryCache;

        public ToggleRecipeController(
            IRecipeManager recipeManager,
            UserManager<User> userManager,
            IMemoryCache memoryCache)
        {
            _recipeManager = recipeManager;
            _userManager = userManager;
            _memoryCache = memoryCache;
        }

        [HttpGet("{identifier}")]
        public async Task<IActionResult> EditData(string identifier)
        {
            var result = await GetRecipeAction(identifier);
            if (result.Error != null)
            {
                return result.Error;
            }
            var vm = new ToggleRecipeViewModel()
            {
                RecipeId = result.Data.RecipeId
            };
            await SetValues(result.Data, vm);

            return View(vm);
        }

        private void SetValues(ToggleRecipeViewModel from, RecipeAction to)
        {
            to.RecipeId = from.RecipeId;
            to.Set((ToggleRecipeData) from);
        }

        private async Task SetValues(RecipeAction from, ToggleRecipeViewModel to)
        {
            var data = from.Get<ToggleRecipeData>();
            to.Recipes = new SelectList(await _recipeManager.GetRecipes(new RecipesQuery()
            {
                UserId = _userManager.GetUserId(User)
            }), nameof(BtcTransmuter.Data.Entities.Recipe.Id), nameof(BtcTransmuter.Data.Entities.Recipe.Name), data.TargetRecipeId);
            to.TargetRecipeId = data.TargetRecipeId;
            to.Option = data.Option;
            to.RecipeId = from.RecipeId;
            
        }

        [HttpPost("{identifier}")]
        public async Task<IActionResult> EditData(string identifier, ToggleRecipeViewModel data)
        {
            var result = await GetRecipeAction(identifier);
            if (result.Error != null)
            {
                return result.Error;
            }

            if (!ModelState.IsValid)
            {
                data.Recipes = new SelectList(await _recipeManager.GetRecipes(new RecipesQuery()
                    {
                        UserId = _userManager.GetUserId(User)
                    }), nameof(BtcTransmuter.Data.Entities.Recipe.Id), nameof(BtcTransmuter.Data.Entities.Recipe.Name),
                    data.TargetRecipeId);
                return View(data);
            }

            var recipeAction = result.Data;
            SetValues(data, recipeAction);

            await _recipeManager.AddOrUpdateRecipeAction(recipeAction);
            return RedirectToAction("EditRecipe", "Recipes", new
            {
                id = recipeAction.RecipeId,
                statusMessage = "Toggle Recipe Action Updated"
            });
        }

        private async Task<(IActionResult Error, RecipeAction Data )> GetRecipeAction(string identifier)
        {
            if (!_memoryCache.TryGetValue(identifier, out RecipeAction data))
            {
                return (RedirectToAction("GetRecipes", "Recipes", new
                {
                    statusMessage = "Error:Data could not be found or data session expired"
                }), null);
            }

            var recipe = await _recipeManager.GetRecipe(data.RecipeId, _userManager.GetUserId(User));

            if (recipe == null)
            {
                return (RedirectToAction("GetRecipes", "Recipes", new
                {
                    statusMessage = "Error:Data could not be found or data session expired"
                }), null);
            }

            return (null, data);
        }


        public class ToggleRecipeViewModel : ToggleRecipeData
        {
            public SelectList Recipes { get; set; }
            public string RecipeId { get; set; }
        }
    }
}