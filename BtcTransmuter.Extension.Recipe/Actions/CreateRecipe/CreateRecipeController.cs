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
    [Route("recipe-plugin/actions/create-recipe")]
    [Authorize]
    public class CreateRecipeController : Controller
    {
        private readonly IRecipeManager _recipeManager;
        private readonly UserManager<User> _userManager;
        private readonly IMemoryCache _memoryCache;

        public CreateRecipeController(
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
            var vm = new CreateRecipeViewModel()
            {
                RecipeId = result.Data.RecipeId
            };
            await SetValues(result.Data, vm);

            return View(vm);
        }

        private void SetValues(CreateRecipeViewModel from, RecipeAction to)
        {
            to.RecipeId = from.RecipeId;
            to.Set((CreateRecipeData) from);
        }

        private async Task SetValues(RecipeAction from, CreateRecipeViewModel to)
        {
            var data = from.Get<CreateRecipeData>();
            to.Recipes = new SelectList(await _recipeManager.GetRecipes(new RecipesQuery()
            {
                UserId = _userManager.GetUserId(User)
            }), nameof(BtcTransmuter.Data.Entities.Recipe.Id), nameof(BtcTransmuter.Data.Entities.Recipe.Name), data.RecipeTemplateId);
            to.RecipeTemplateId = data.RecipeTemplateId;
            to.Enable = data.Enable;
            to.RecipeId = from.RecipeId;
            
        }

        [HttpPost("{identifier}")]
        public async Task<IActionResult> EditData(string identifier, CreateRecipeViewModel data)
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
                    data.RecipeTemplateId);
                return View(data);
            }

            var recipeAction = result.Data;
            SetValues(data, recipeAction);

            await _recipeManager.AddOrUpdateRecipeAction(recipeAction);
            return RedirectToAction("EditRecipe", "Recipes", new
            {
                id = recipeAction.RecipeId,
                statusMessage = "Create Recipe Action Updated"
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


        public class CreateRecipeViewModel : CreateRecipeData
        {
            public SelectList Recipes { get; set; }
            public string RecipeId { get; set; }
        }
    }
}