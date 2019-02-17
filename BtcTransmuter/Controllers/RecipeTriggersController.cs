using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
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
    [Route("recipes/{id}/triggers")]
    public class RecipeTriggersController : Controller
    {
        private readonly IRecipeManager _recipeManager;
        private readonly UserManager<User> _userManager;
        private readonly IEnumerable<ITriggerDescriptor> _triggerDescriptors;

        public RecipeTriggersController(IRecipeManager recipeManager, UserManager<User> userManager,
            IEnumerable<ITriggerDescriptor> triggerDescriptors)
        {
            _recipeManager = recipeManager;
            _userManager = userManager;
            _triggerDescriptors = triggerDescriptors;
        }

        [HttpGet("{recipeTriggerId?}")]
        public async Task<IActionResult> EditRecipeTrigger(string id, string recipeTriggerId, string statusMessage)
        {
            var recipe = await _recipeManager.GetRecipe(id, _userManager.GetUserId(User));
            if (recipe == null)
            {
                return GetNotFoundActionResult();
            }

            return View(new EditRecipeTriggerViewModel()
            {
                RecipeId = id,
                TriggerId = recipe.RecipeTrigger?.TriggerId,
                RecipeTrigger = recipe.RecipeTrigger,
                StatusMessage = statusMessage,
                Triggers = new SelectList(_triggerDescriptors, nameof(ITriggerDescriptor.TriggerId),
                    nameof(ITriggerDescriptor.Name), recipe.RecipeTrigger?.TriggerId)
            });
        }

        [HttpPost("{recipeTriggerId?}")]
        public async Task<IActionResult> EditRecipeTrigger(string id, string recipeTriggerId,
            EditRecipeTriggerViewModel model)
        {
            var recipe = await _recipeManager.GetRecipe(id, _userManager.GetUserId(User));
            if (recipe == null)
            {
                return GetNotFoundActionResult();
            }


            if (!ModelState.IsValid)
            {
                model.Triggers = new SelectList(_triggerDescriptors, nameof(ITriggerDescriptor.TriggerId),
                    nameof(ITriggerDescriptor.Name), model.TriggerId);

                return View(model);
            }

            var recipeTrigger = model.RecipeTrigger;

            if (string.IsNullOrEmpty(recipeTriggerId) || recipe.RecipeTrigger.TriggerId != model.TriggerId)
            {
                recipeTrigger = new RecipeTrigger()
                {
                    RecipeId = id,
                    TriggerId = model.TriggerId,
                };
            }

            var serviceDescriptor =
                _triggerDescriptors.Single(descriptor =>
                    descriptor.TriggerId == recipeTrigger.TriggerId);
            return await serviceDescriptor.EditData(recipeTrigger);
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