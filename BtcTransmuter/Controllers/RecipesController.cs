using System;
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
    [Route("recipes")]
    public class RecipesController : Controller
    {
        private readonly IRecipeManager _recipeManager;
        private readonly UserManager<User> _userManager;

        public RecipesController(IRecipeManager recipeManager, UserManager<User> userManager)
        {
            _recipeManager = recipeManager;
            _userManager = userManager;
        }

        [HttpGet("")]
        public async Task<IActionResult> GetRecipes([FromQuery] string statusMessage = null)
        {
            var recipes = await _recipeManager.GetRecipes(new RecipesQuery()
            {
                UserId = _userManager.GetUserId(User)
            });

            return View(new GetRecipesViewModel()
            {
                StatusMessage = statusMessage,
                Recipes = recipes
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
        public IActionResult CreateRecipe(string statusMessage)
        {
            return View(new CreateRecipeViewModel()
            {
                StatusMessage = statusMessage
            });
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateRecipe(CreateRecipeViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                return View(viewModel);
            }

            var recipe = new Recipe()
            {
                Enabled = false,
                Name = viewModel.Name,
                Description = viewModel.Description,
                UserId = _userManager.GetUserId(User)
            };
            await _recipeManager.AddOrUpdateRecipe(recipe);
            if (string.IsNullOrEmpty(recipe.Id))
            {
                ModelState.AddModelError(string.Empty, "Could not save recipe");
                return View(viewModel);
            }

            return RedirectToAction("EditRecipe", new {id = recipe.Id, statusMessage = "Recipe created"});
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> EditRecipe(string id, string statusMessage)
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
            return View(new EditRecipeViewModel()
            {
                StatusMessage = statusMessage,
                Name = recipe.Name,
                Enabled = recipe.Enabled,
                Description = recipe.Description,
                Actions = recipe.RecipeActions,
                Trigger = recipe.RecipeTrigger
            });
        }

        [HttpPost("{id}")]
        public async Task<IActionResult> EditRecipe(string id, EditRecipeViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                return View(viewModel);
            }

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

            recipe.Name = recipe.Name;
            recipe.Enabled = recipe.Enabled;
            recipe.Description = recipe.Description;

            await _recipeManager.AddOrUpdateRecipe(recipe);

            return RedirectToAction("EditRecipe", new {id = recipe.Id, statusMessage = "Recipe edited"});
        }
    }
}