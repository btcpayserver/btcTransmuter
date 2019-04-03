using System.Threading.Tasks;
using BtcTransmuter.Abstractions.Actions;
using BtcTransmuter.Abstractions.ExternalServices;
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
    [Route("recipe-plugin/actions/[controller]")]
    [Authorize]
    public class
        ToggleRecipeController : BaseActionController<ToggleRecipeController.ToggleRecipeViewModel, ToggleRecipeData>
    {
        public ToggleRecipeController(IMemoryCache memoryCache, UserManager<User> userManager,
            IRecipeManager recipeManager, IExternalServiceManager externalServiceManager) : base(memoryCache,
            userManager, recipeManager, externalServiceManager)
        {
        }

        public class ToggleRecipeViewModel : ToggleRecipeData, IActionViewModel
        {
            public SelectList Recipes { get; set; }
            public string RecipeId { get; set; }
            public string RecipeActionIdInGroupBeforeThisOne { get; set; }
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

        protected override async Task<(RecipeAction ToSave, ToggleRecipeViewModel showViewModel)> BuildModel(
            ToggleRecipeViewModel viewModel, RecipeAction mainModel)
        {
            if (ModelState.IsValid)
            {
                mainModel.Set<ToggleRecipeData>(viewModel);
                return (mainModel, null);
            }

            viewModel.Recipes = new SelectList(
                await _recipeManager.GetRecipes(new RecipesQuery() {UserId = _userManager.GetUserId(User)}),
                nameof(BtcTransmuter.Data.Entities.Recipe.Id), nameof(BtcTransmuter.Data.Entities.Recipe.Name),
                viewModel.TargetRecipeId);
            return (null, viewModel);
        }
    }
}