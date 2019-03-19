using System;
using System.Linq;
using System.Threading.Tasks;
using BtcTransmuter.Abstractions.Recipes;
using BtcTransmuter.Data.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace BtcTransmuter.Abstractions.Actions
{
    public interface IActionViewModel
    {
        string RecipeId { get; set; }
        string RecipeActionIdInGroupBeforeThisOne { get; set; }
    }

    [Authorize]
    public abstract class BaseActionController<TViewModel, TRecipeActionData> : Controller
        where TViewModel : TRecipeActionData, IActionViewModel
    {
        private readonly IMemoryCache _memoryCache;
        protected readonly UserManager<User> _userManager;
        protected readonly IRecipeManager _recipeManager;
        private string RecipeActionIdInGroupBeforeThisOne;

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

            var vm = await BuildViewModel(result.Data);

            vm.RecipeId = result.Data.RecipeId;
            vm.RecipeActionIdInGroupBeforeThisOne = RecipeActionIdInGroupBeforeThisOne;
            return View(vm);
        }

        [HttpPost("{identifier}")]
        public virtual async Task<IActionResult> EditData(string identifier, TViewModel data)
        {
            var result = await GetRecipeAction(identifier);
            if (result.Error != null)
            {
                return result.Error;
            }

            var modelResult = await BuildModel(data, result.Data);

            if (modelResult.showViewModel != null)
            {
                modelResult.showViewModel.RecipeId = result.Data.RecipeId;
                modelResult.showViewModel.RecipeActionIdInGroupBeforeThisOne = RecipeActionIdInGroupBeforeThisOne;
                return View(modelResult.showViewModel);
            }

            await _recipeManager.AddOrUpdateRecipeAction(modelResult.ToSave);
            return RedirectToAction("EditRecipe", "Recipes", new
            {
                id = modelResult.ToSave.RecipeId,
                statusMessage = "Action Updated"
            });
        }

        protected abstract Task<TViewModel> BuildViewModel(RecipeAction recipeAction);

        protected abstract Task<(RecipeAction ToSave, TViewModel showViewModel)> BuildModel(
            TViewModel viewModel, RecipeAction mainModel);

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

            if (!string.IsNullOrEmpty(data.RecipeActionGroupId))
            {
                var recipeActionGroup = recipe.RecipeActionGroups.Single(group =>
                    group.Id.Equals(data.RecipeActionGroupId, StringComparison.InvariantCultureIgnoreCase));
                var actions = recipeActionGroup.RecipeActions.OrderBy(action => action.Order);

                var matched = actions.Select((action, i) => (action, i))
                    .Where(tuple => tuple.Item1.ActionId == data.Id);

                var index = matched.Any()
                    ? actions.Select((action, i) => (action, i))
                        .Where(tuple => tuple.Item1.ActionId == data.Id)
                        .Select(tuple => tuple.Item2)
                        .FirstOrDefault()
                    : -1;
                if (index == -1 && actions.Any())
                {
                    RecipeActionIdInGroupBeforeThisOne = actions.Last().Id;
                }
                else if (index > 0)
                {
                    RecipeActionIdInGroupBeforeThisOne = actions.ElementAt(index - 1).Id;
                }
            }

            return (null, data);
        }

        protected string GetUserId()
        {
            return _userManager.GetUserId(User);
        }
    }
}