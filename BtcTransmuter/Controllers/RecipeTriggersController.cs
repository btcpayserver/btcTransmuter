using System.Collections.Generic;
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

        public RecipeTriggersController(IRecipeManager recipeManager, UserManager<User> userManager, IEnumerable<ITriggerDescriptor> triggerDescriptors)
        {
            _recipeManager = recipeManager;
            _userManager = userManager;
            _triggerDescriptors = triggerDescriptors;
        }
        [HttpGet("{recipeTriggerId?}")]
        public async Task<IActionResult> EditRecipeTrigger(string id, string recipeTriggerId, string statusMessage)
        {
            var recipe = await GetRecipeForUser(id);
            if (recipe == null)
            {
                return GetNotFoundActionResult();
            }

            return View(new EditRecipeTriggerViewModel()
            {
                StatusMessage = statusMessage
            });
        }

        [HttpPost("{recipeTriggerId?}")]
        public async Task<IActionResult> EditRecipeTrigger(string id, string recipeTriggerId,  EditRecipeTriggerViewModel model)
        {
            var recipe = await GetRecipeForUser(id);
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

            return View();
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

    public class EditRecipeTriggerViewModel
    {
        public string TriggerId { get; set; }
        public SelectList Triggers { get; set; }
        public string StatusMessage { get; set; }
        
    }
}