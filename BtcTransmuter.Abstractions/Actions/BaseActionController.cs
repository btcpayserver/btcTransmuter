using System.Threading.Tasks;
using BtcTransmuter.Abstractions.ExternalServices;
using BtcTransmuter.Abstractions.Recipes;
using BtcTransmuter.Data.Entities;
using BtcTransmuter.Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace BtcTransmuter.Abstractions.Actions
{
    [Authorize]
    public abstract class BaseActionController<TViewModel, TRecipeActionData> : Controller
        where TViewModel : TRecipeActionData
    {
        private readonly IMemoryCache _memoryCache;
        protected readonly UserManager<User> _userManager;
        protected readonly IRecipeManager _recipeManager;

        protected BaseActionController(IMemoryCache memoryCache,
            UserManager<User> userManager,
            IRecipeManager recipeManager)
        {
            _memoryCache = memoryCache;
            _userManager = userManager;
            _recipeManager = recipeManager;
        }

        [HttpGet("{identifier}")]
        public async Task<IActionResult> EditData(string identifier)
        {
            var result = await GetRecipeAction(identifier);
            if (result.Error != null)
            {
                return result.Error;
            }

            return View(await BuildViewModel(result.Data));
        }

        [HttpPost("{identifier}")]
        public virtual async Task<IActionResult> EditData(string identifier, TViewModel data)
        {
            var result = await GetRecipeAction(identifier);
            if (result.Error != null)
            {
                return result.Error;
            }

            if (!ModelState.IsValid)
            {
                return View(await BuildViewModel(data));
            }

            var recipeAction = result.Data;
            await SetRecipeActionFromViewModel(data, recipeAction);

            await _recipeManager.AddOrUpdateRecipeAction(recipeAction);
            return RedirectToAction("EditRecipe", "Recipes", new
            {
                id = recipeAction.RecipeId,
                statusMessage = "Action Updated"
            });
        }

        protected virtual Task SetRecipeActionFromViewModel(TViewModel from, RecipeAction to)
        {
            if (from is IUseExternalService fromExternalService)
            {
                to.ExternalServiceId = fromExternalService.ExternalServiceId;
            }

            to.Set((TRecipeActionData) from);
            return Task.CompletedTask;
        }

        protected abstract Task<TViewModel> BuildViewModel(RecipeAction recipeAction);
        protected abstract Task<TViewModel> BuildViewModel(TViewModel recipeAction);

        private async Task<(IActionResult Error, RecipeAction Data )> GetRecipeAction(string identifier)
        {
            if (!_memoryCache.TryGetValue(identifier, out RecipeAction data))
            {
                return (RedirectToAction("GetRecipes", "Recipes", new
                {
                    statusMessage = "Error:Data could not be found or data session expired"
                }), null);
            }

            var recipe = await _recipeManager.GetRecipe(data.RecipeId, GetUserId());

            if (recipe == null)
            {
                return (RedirectToAction("GetRecipes", "Recipes", new
                {
                    statusMessage = "Error:Data could not be found or data session expired"
                }), null);
            }

            return (null, data);
        }

        protected string GetUserId()
        {
            return _userManager.GetUserId(User);
        }
    }
}