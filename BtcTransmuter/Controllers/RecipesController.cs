using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BtcTransmuter.Abstractions.Actions;
using BtcTransmuter.Abstractions.ExternalServices;
using BtcTransmuter.Abstractions.Recipes;
using BtcTransmuter.Abstractions.Triggers;
using BtcTransmuter.Data.Entities;
using BtcTransmuter.Extension.Email.Actions.SendEmail;
using BtcTransmuter.Extension.Email.Triggers.ReceivedEmail;
using BTCPayServer.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace BtcTransmuter.Controllers
{
    [Authorize]
    public class RecipesController : Controller
    {
        private readonly IRecipeManager _recipeManager;
        private readonly UserManager<User> _userManager;
        private readonly IEnumerable<IActionDescriptor> _actionDescriptors;
        private readonly IEnumerable<ITriggerDescriptor> _triggerDescriptors;
        private readonly IEnumerable<IExternalServiceDescriptor> _externalServiceDescriptors;

        public RecipesController(IRecipeManager recipeManager, UserManager<User> userManager,
            IEnumerable<IActionDescriptor> actionDescriptors, 
            IEnumerable<ITriggerDescriptor> triggerDescriptors,
            IEnumerable<IExternalServiceDescriptor> externalServiceDescriptors)
        {
            _recipeManager = recipeManager;
            _userManager = userManager;
            _actionDescriptors = actionDescriptors;
            _triggerDescriptors = triggerDescriptors;
            _externalServiceDescriptors = externalServiceDescriptors;
        }

        [HttpGet("")]
        public async Task<IActionResult> GetRecipes([FromQuery]string statusMessage = null)
        {
            var recipes = await _recipeManager.GetRecipes(new RecipesQuery()
            {
                UserId = _userManager.GetUserId(User)
            });
            recipes = new List<Recipe>()
            {
                new Recipe()
                {
                    Id = "1",
                    Name = "Test Recipe",
                    UserId = _userManager.GetUserId(User),
                    RecipeTrigger = new RecipeTrigger()
                    {
                        Id = "1",
                        RecipeId = "1",
                        TriggerId = new ReceivedEmailTrigger().Id,
                        ExternalServiceId = "1"
                    },
                    Description = "test desc",
                    Enabled = true,
                    RecipeInvocations = new List<RecipeInvocation>(),
                    RecipeActions = new List<RecipeAction>()
                    {
                        new RecipeAction()
                        {
                            Id = "2",
                            RecipeId = "1",
                            ExternalServiceId = "2",
                            ActionId = new SendEmailActionDescriptor().ActionId,
                            
                        }
                    }
                }
            };
            return View(new GetRecipesViewModel()
            {
                StatusMessage = statusMessage,
                Recipes = recipes,
                ActionDescriptors = _actionDescriptors,
                TriggerDescriptors = _triggerDescriptors,
                ExternalServiceDescriptors = _externalServiceDescriptors
            });
        }

        [HttpGet("{id}/remove")]
        public async Task<IActionResult> RemoveRecipe(string id)
        {
            var recipe = await _recipeManager.GetRecipes(new RecipesQuery()
            {
                UserId = _userManager.GetUserId(User),
                RecipeId = id
            });
            if (!recipe.Any())
            {
                return RedirectToAction("GetRecipes", new
                {
                    statusMessage = new StatusMessageModel()
                    {
                        Message = "Recipe not found",
                        Severity = StatusMessageModel.StatusSeverity.Error
                    }.ToString()
                });
            }

            return View(new RemoveRecipeViewModel()
            {
                Recipe = recipe.First()
            });
        }

        [HttpPost("{id}/remove")]
        public async Task<IActionResult> RemoveRecipePost(string id)
        {
            var recipe = await _recipeManager.GetRecipes(new RecipesQuery()
            {
                UserId = _userManager.GetUserId(User),
                RecipeId = id
            });
            if (!recipe.Any())
            {
                return RedirectToAction("GetRecipes", new
                {
                    statusMessage = new StatusMessageModel()
                    {
                        Message = "Recipe not found",
                        Severity = StatusMessageModel.StatusSeverity.Error
                    }.ToString()
                });
            }

            await _recipeManager.RemoveRecipe(id);
            return RedirectToAction("GetRecipes", new
            {
                statusMessage = new StatusMessageModel()
                {
                    Message = $"Recipe {recipe.First().Name} deleted successfully",
                    Severity = StatusMessageModel.StatusSeverity.Success
                }.ToString()
            });
        }


        [HttpGet("{id}/logs")]
        public async Task<IActionResult> GetRecipeLogs(string id)
        {
            var recipes = await _recipeManager.GetRecipes(new RecipesQuery()
            {
                UserId = _userManager.GetUserId(User),
                RecipeId = id
            });

            if (!recipes.Any())
            {
                return RedirectToAction("GetRecipes", new
                {
                    statusMessage = new StatusMessageModel()
                    {
                        Message = "Recipe not found",
                        Severity = StatusMessageModel.StatusSeverity.Error
                    }.ToString()
                });
            }

            var recipe = recipes.First();
            return View(new GetRecipeLogsViewModel()
            {
                Name = recipe.Name,
                RecipeInvocations = recipe.RecipeInvocations
            });
        }

        [HttpGet("create")]
        public IActionResult CreateRecipe()
        {
            return View(new CreateRecipeViewModel());
        }

        [HttpPost("create")]
        public IActionResult CreateRecipePost(CreateRecipeViewModel viewModel)
        {
            return Ok();
        }
    }
}