using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BtcTransmuter.Abstractions.Actions;
using BtcTransmuter.Abstractions.Extensions;
using BtcTransmuter.Abstractions.Recipes;
using BtcTransmuter.Data.Entities;
using BtcTransmuter.Models;
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
        public async Task<IActionResult> EditRecipeAction(string id, string recipeActionId, string statusMessage, string recipeActionGroupId)
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

            return this.ViewOrJson(new EditRecipeActionViewModel()
            {
                RecipeId = id,
                ActionId = recipeAction?.ActionId,
                RecipeActionGroupId = recipeActionGroupId,
                RecipeAction = recipeAction,
                StatusMessage = statusMessage,
                Actions = new SelectList(_actionDescriptors, nameof(IActionDescriptor.ActionId),
                    nameof(IActionDescriptor.Name), recipeAction?.ActionId)
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
            var recipeActionGroup = string.IsNullOrEmpty(model.RecipeActionGroupId) ?
            null :
            recipe.RecipeActionGroups.Single(x => x.Id == model.RecipeActionGroupId);
            var recipeAction = recipeActionGroup == null ?
            recipe.RecipeActions.SingleOrDefault(action => action.Id == recipeActionId) :
            recipeActionGroup.RecipeActions.SingleOrDefault(action => action.Id == recipeActionId);

            if (!ModelState.IsValid)
            {
                model.RecipeAction = recipeAction;
                model.Actions = new SelectList(_actionDescriptors, nameof(IActionDescriptor.ActionId),
                    nameof(IActionDescriptor.Name), model.ActionId);
                return this.ViewOrBadRequest(model);
            }

            if (string.IsNullOrEmpty(recipeActionId) || recipeAction.ActionId != model.ActionId)
            {
                recipeAction = new RecipeAction()
                {
                    Id = recipeActionId,
                    RecipeId = id,
                    RecipeActionGroupId = model.RecipeActionGroupId,
                    ActionId = model.ActionId,
                };
            }

            var serviceDescriptor =
                _actionDescriptors.Single(descriptor =>
                    descriptor.ActionId == recipeAction.ActionId);
            return await serviceDescriptor.EditData(recipeAction);
        }


        [HttpGet("{recipeActionId}/remove")]
        public async Task<IActionResult> RemoveRecipeAction(string id, string recipeActionId)
        {
            var recipe = await _recipeManager.GetRecipe(id, _userManager.GetUserId(User));
            if (recipe == null)
            {
                return GetNotFoundActionResult();
            }

            var recipeAction = recipe.RecipeActions.SingleOrDefault(action => action.Id == recipeActionId);
            if (recipeAction == null)
            {
                return GetNotFoundActionResult();
            }

            return View(new RemoveRecipeActionViewModel()
            {
                RecipeAction = recipeAction
            });
        }

        [HttpPost("{recipeActionId}/remove")]
        public async Task<IActionResult> RemoveRecipeActionPost(string id, string recipeActionId)
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

            await _recipeManager.RemoveRecipeAction(recipeActionId);
            if (this.IsApi())
            {
                return Ok();
            }
            return RedirectToAction("EditRecipe", "Recipes", new
            {
                id,
                statusMessage = new StatusMessageModel()
                {
                    Message = $"Recipe Action removed",
                    Severity = StatusMessageModel.StatusSeverity.Success
                }.ToString()
            });
        }

        private ActionResult GetNotFoundActionResult()
        {
            if (this.IsApi())
            {
                return NotFound();
            }
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