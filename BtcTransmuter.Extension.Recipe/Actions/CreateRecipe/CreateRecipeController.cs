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

namespace BtcTransmuter.Extension.Recipe.Actions.CreateRecipe
{
    [Route("recipe-plugin/actions/[controller]")]
    [Authorize]
    public class
        CreateRecipeController : BaseActionController<CreateRecipeController.CreateRecipeViewModel, CreateRecipeData>
    {
        public CreateRecipeController(IMemoryCache memoryCache, UserManager<User> userManager,
            IRecipeManager recipeManager, IExternalServiceManager externalServiceManager) : base(memoryCache, userManager, recipeManager, externalServiceManager)
        {
        }

        protected override async Task<CreateRecipeViewModel> BuildViewModel(RecipeAction from)
        {
            var data = from.Get<CreateRecipeData>();
            return new CreateRecipeViewModel
            {
                RecipeId = from.RecipeId,
                Recipes = new SelectList(
                    await _recipeManager.GetRecipes(new RecipesQuery() {UserId = _userManager.GetUserId(User)}),
                    nameof(BtcTransmuter.Data.Entities.Recipe.Id), nameof(BtcTransmuter.Data.Entities.Recipe.Name),
                    data.RecipeTemplateId),
                RecipeTemplateId = data.RecipeTemplateId,
                Enable = data.Enable
            };
        }

        protected override async Task<(RecipeAction ToSave, CreateRecipeViewModel showViewModel)> BuildModel(
            CreateRecipeViewModel viewModel, RecipeAction mainModel)
        {
            if (ModelState.IsValid)
            {
                mainModel.Set<CreateRecipeData>(viewModel);
                return (mainModel, null);
            }

            viewModel.Recipes = new SelectList(
                await _recipeManager.GetRecipes(new RecipesQuery() {UserId = _userManager.GetUserId(User)}),
                nameof(BtcTransmuter.Data.Entities.Recipe.Id), nameof(BtcTransmuter.Data.Entities.Recipe.Name),
                viewModel.RecipeTemplateId);
            return (null, viewModel);
        }

        public class CreateRecipeViewModel : CreateRecipeData, IActionViewModel
        {
            public SelectList Recipes { get; set; }
            public string RecipeId { get; set; }
            public string RecipeActionIdInGroupBeforeThisOne { get; set; }
        }
    }
}