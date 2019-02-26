using System.Threading.Tasks;
using BtcTransmuter.Abstractions.Recipes;
using BtcTransmuter.Data.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace BtcTransmuter.Abstractions.Triggers
{
    [Authorize]
    public abstract class BaseTriggerController<TViewModel, TRecipeTriggerParameters>: Controller
        where TViewModel : TRecipeTriggerParameters
    {
        private readonly IRecipeManager _recipeManager;
        private readonly UserManager<User> _userManager;
        private readonly IMemoryCache _memoryCache;

        protected BaseTriggerController(
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
            var result = await GetRecipeTrigger(identifier);
            if (result.Error != null)
            {
                return result.Error;
            }

            return View(await BuildViewModel(result.Data));
        }
        
        protected abstract Task<TViewModel> BuildViewModel(RecipeTrigger data);
        protected abstract Task<(RecipeTrigger ToSave, TViewModel showViewModel)> BuildModel(
            TViewModel viewModel, RecipeTrigger mainModel);

        [HttpPost("{identifier}")]
        public async Task<IActionResult> EditData(string identifier, TViewModel viewmodel)
        {
            var result = await GetRecipeTrigger(identifier);
            if (result.Error != null)
            {
                return result.Error;
            }

            var modelResult = await BuildModel(viewmodel, result.Data);

            if (modelResult.showViewModel != null)
            {
                return View(modelResult.showViewModel);
            }
            
            await _recipeManager.AddOrUpdateRecipeTrigger(modelResult.ToSave);
            return RedirectToAction("EditRecipe", "Recipes", new
            {
                id = modelResult.ToSave.RecipeId,
                statusMessage = "Trigger Updated"
            });
        }

        protected string GetUserId()
        {
            return _userManager.GetUserId(User);
        }

        private async Task<(IActionResult Error, RecipeTrigger Data )> GetRecipeTrigger(string identifier)
        {
            if (!_memoryCache.TryGetValue(identifier, out RecipeTrigger data))
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
    }
}