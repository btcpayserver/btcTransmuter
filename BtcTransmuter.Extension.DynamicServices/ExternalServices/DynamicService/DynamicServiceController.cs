using System;
using System.Linq;
using System.Threading.Tasks;
using BtcTransmuter.Abstractions.ExternalServices;
using BtcTransmuter.Abstractions.Recipes;
using BtcTransmuter.Data.Entities;
using BtcTransmuter.Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Caching.Memory;
namespace BtcTransmuter.Extension.DynamicService.ExternalServices.DynamicService
{
    [Route("DynamicService-plugin/external-services/DynamicService")]
    [Authorize]
    public class DynamicServiceController : BaseExternalServiceController<EditDynamicServiceDataViewModel>
    {
        private readonly UserManager<User> _userManager;
        private readonly IRecipeManager _recipeManager;

        public DynamicServiceController(IExternalServiceManager externalServiceManager, UserManager<User> userManager,
            IMemoryCache memoryCache, IRecipeManager recipeManager) : base(externalServiceManager, userManager, memoryCache)
        {
            _userManager = userManager;
            _recipeManager = recipeManager;
        }

        protected override string ExternalServiceType => DynamicServiceService.DynamicServiceServiceType;

        protected override async Task<EditDynamicServiceDataViewModel> BuildViewModel(ExternalServiceData data)
        {
            var client = new DynamicServiceService(data, null, null, null);
            var clientData = client.GetData();

            var recipes = await _recipeManager.GetRecipes(new RecipesQuery()
            {
                UserId = _userManager.GetUserId(User)
            });
            var recipe = string.IsNullOrEmpty(clientData.RecipeId)
                ? null
                : recipes.SingleOrDefault(recipe1 =>
                    recipe1.Id.Equals(clientData.RecipeId, StringComparison.InvariantCultureIgnoreCase));
            return new EditDynamicServiceDataViewModel()
            {
                Value = clientData.Value,
                RecipeId = clientData.RecipeId,
                RecipeActionId = clientData.RecipeActionId,
                RecipeActionGroupId = clientData.RecipeActionGroupId,
                Recipes = new SelectList(recipes, nameof(Recipe.Id), nameof(Recipe.Name), clientData.RecipeId),
                RecipeActions =recipe == null? null :  new SelectList(recipe.RecipeActions, nameof(RecipeAction.Id), nameof(RecipeAction.ActionId), clientData.RecipeActionId),
                RecipeActionGroups = recipe == null? null :  new SelectList(recipe.RecipeActionGroups, nameof(RecipeActionGroup.Id), nameof(RecipeActionGroup.Id), clientData.RecipeActionGroupId),
            };
        }

        protected override async Task<(ExternalServiceData ToSave, EditDynamicServiceDataViewModel showViewModel)>
            BuildModel(EditDynamicServiceDataViewModel viewModel, ExternalServiceData mainModel)
        {
            if (string.IsNullOrEmpty(viewModel.RecipeActionId) && string.IsNullOrEmpty(viewModel.RecipeActionGroupId))
            {
                ModelState.AddModelError(string.Empty, "please select a recipe action or a recipe action group ");
            }
            
            if (!ModelState.IsValid)
            {
                var recipes = await _recipeManager.GetRecipes(new RecipesQuery()
                {
                    UserId = _userManager.GetUserId(User)
                });
                var recipe = string.IsNullOrEmpty(viewModel.RecipeId)
                    ? null
                    : recipes.SingleOrDefault(recipe1 =>
                        recipe1.Id.Equals(viewModel.RecipeId, StringComparison.InvariantCultureIgnoreCase));
                
                
                
                return (null, new EditDynamicServiceDataViewModel()
                {
                    Value = viewModel.Value,
                    RecipeId = viewModel.RecipeId,
                    RecipeActionId = viewModel.RecipeActionId,
                    RecipeActionGroupId = viewModel.RecipeActionGroupId,
                    Recipes = new SelectList(recipes, nameof(Recipe.Id), nameof(Recipe.Name), viewModel.RecipeId),
                    RecipeActions =recipe == null? null :  new SelectList(recipe.RecipeActions, nameof(RecipeAction.Id), nameof(RecipeAction.ActionId), viewModel.RecipeActionId),
                    RecipeActionGroups = recipe == null? null :  new SelectList(recipe.RecipeActionGroups, nameof(RecipeActionGroup.Id), nameof(RecipeActionGroup.Id), viewModel.RecipeActionGroupId),

                });
            }
           
            mainModel.Set(viewModel);
            return (mainModel, null);
        }
    }
}