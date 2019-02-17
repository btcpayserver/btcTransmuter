using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BtcTransmuter.Abstractions.Actions;
using BtcTransmuter.Abstractions.Recipes;
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

        public RecipeActionsController(IRecipeManager recipeManager, UserManager<User> userManager,
            IEnumerable<IActionDescriptor> actionDescriptors)
        {
            _recipeManager = recipeManager;
            _userManager = userManager;
            _actionDescriptors = actionDescriptors;
        }

        [HttpGet("{recipeActionId?}")]
        public async Task<IActionResult> EditRecipeAction(string id, string recipeActionId, string statusMessage)
        {
            var recipe = await _recipeManager.GetRecipe(id, _userManager.GetUserId(User));
            if (recipe == null)
            {
                return GetNotFoundActionResult();
            }

            RecipeAction recipeAction = null;
            if (!string.IsNullOrEmpty(recipeActionId))
            {
                recipeAction = recipe.RecipeActions.SingleOrDefault(action => action.Id == recipeActionId);
                if (recipeAction == null)
                {
                    return GetNotFoundActionResult();
                }
            }

            return View(new EditRecipeActionViewModel()
            {
                RecipeId = id,
                ActionId = recipeAction?.ActionId,
                RecipeAction = recipeAction,
                StatusMessage = statusMessage,
                Actions = new SelectList(_actionDescriptors, nameof(IActionDescriptor.ActionId),
                    nameof(IActionDescriptor.Name),  recipeAction?.ActionId)
            });
        }

        [HttpPost("{recipeActionId?}")]
        public async Task<IActionResult> EditRecipeAction(string id, string recipeActionId,
            EditRecipeActionViewModel model)
        {
            var recipe = await _recipeManager.GetRecipe(id, _userManager.GetUserId(User));
            if (recipe == null)
            {
                return GetNotFoundActionResult();
            }

            var recipeAction = recipe.RecipeActions.SingleOrDefault(action => action.Id == recipeActionId);

            if (!ModelState.IsValid)
            {
                model.RecipeAction = recipeAction;
                model.Actions = new SelectList(_actionDescriptors, nameof(IActionDescriptor.ActionId),
                    nameof(IActionDescriptor.Name), model.ActionId);
                return View(model);
            }

            if (string.IsNullOrEmpty(recipeActionId) || recipeAction.ActionId != model.ActionId)
            {
                recipeAction = new RecipeAction()
                {
                    Id = recipeActionId,
                    RecipeId = id,
                    ActionId = model.ActionId,
                };
            }

            var serviceDescriptor =
                _actionDescriptors.Single(descriptor =>
                    descriptor.ActionId == recipeAction.ActionId);
            return await serviceDescriptor.EditData(recipeAction);
        }

        private RedirectToActionResult GetNotFoundActionResult()
        {
            return RedirectToAction("GetRecipes", "Recipes", new
            {
                statusMessage = new StatusMessageModel()
                {
                    Message = "Recipe not found",
                    Severity = StatusMessageModel.StatusSeverity.Error
                }.ToString()
            });
        }
    }
}