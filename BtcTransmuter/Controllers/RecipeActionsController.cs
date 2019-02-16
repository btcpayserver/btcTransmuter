using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BtcTransmuter.Abstractions.Actions;
using BtcTransmuter.Abstractions.Recipes;
using BtcTransmuter.Abstractions.Triggers;
using BtcTransmuter.Data.Entities;
using BTCPayServer.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BtcTransmuter.Controllers
{
    [Authorize]
    [Route("recipes/{id}/actions")]
    public class RecipeActionsController : Controller
    {
        private readonly IRecipeManager _recipeManager;
        private readonly UserManager<User> _userManager;
        private readonly IEnumerable<IActionDescriptor> _actionDescriptors;

        public RecipeActionsController(IRecipeManager recipeManager, UserManager<User> userManager, IEnumerable<IActionDescriptor> actionDescriptors)
        {
            _recipeManager = recipeManager;
            _userManager = userManager;
            _actionDescriptors = actionDescriptors;
        }
        [HttpGet("{recipeActionId?}")]
        public async Task<IActionResult> EditRecipeAction(string id, string recipeActionId, string statusMessage)
        {
            var recipe = await GetRecipeForUser(id);
            if (recipe == null)
            {
                return GetNotFoundActionResult();
            }

            var model = new EditRecipeActionViewModel()
            {
                Actions = new SelectList(_actionDescriptors, nameof(IActionDescriptor.ActionId),
                    nameof(IActionDescriptor.Name), recipeActionId),
                StatusMessage = statusMessage
            };
            if (string.IsNullOrEmpty(recipeActionId))
            {
                return View(model);
            }

            var recipeAction = recipe.RecipeActions.Single(action => action.Id == recipeActionId);
            if (recipeAction == null)
            {
                return GetNotFoundActionResult();
            }
            

            return View(model);
        }

        [HttpPost("{recipeActionId?}")]
        public async Task<IActionResult> EditRecipeAction(string id, string actionId, EditRecipeActionViewModel model)
        {
            var recipe = await GetRecipeForUser(id);
            if (recipe == null)
            {
                return GetNotFoundActionResult();
            }
            
            
            if (!ModelState.IsValid)
            {
                model.Actions = new SelectList(_actionDescriptors, nameof(IActionDescriptor.ActionId),
                    nameof(IActionDescriptor.Name), model.ActionId);
                return View(model);
            }

            return RedirectToAction("EditRecipe","Recipes", new { id=id});
        }

        private async Task<Recipe> GetRecipeForUser(string recipeId)
        {
            var recipes = await _recipeManager.GetRecipes(new RecipesQuery()
            {
                UserId = _userManager.GetUserId(User),
                RecipeId = recipeId
            });

            return recipes.FirstOrDefault();
        }

        private RedirectToActionResult GetNotFoundActionResult()
        {
            return RedirectToAction("GetRecipes","Recipes", new
            {
                statusMessage = new StatusMessageModel()
                {
                    Message = "Recipe not found",
                    Severity = StatusMessageModel.StatusSeverity.Error
                }.ToString()
            });
        }
    }

    public class EditRecipeActionViewModel
    {
        public string ActionId { get; set; }
        public SelectList Actions { get; set; }
        public string StatusMessage { get; set; }
        
    }
}