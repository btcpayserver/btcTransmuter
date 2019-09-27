using System;
using System.Linq;
using System.Threading.Tasks;
using BtcTransmuter.Abstractions.Models;
using BtcTransmuter.Abstractions.Recipes;
using BtcTransmuter.Data.Entities;
using BtcTransmuter.Models;
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
        public virtual async Task<IActionResult> GetRecipes([FromQuery] string statusMessage = null)
        {
            var recipes = await _recipeManager.GetRecipes(new RecipesQuery()
            {
                UserId = GetUserId()
            });

            return View("GetRecipes",new GetRecipesViewModel()
            {
                StatusMessage = statusMessage,
                Recipes = recipes
            });
        }

        [HttpGet("{id}/remove")]
        public virtual async Task<IActionResult> RemoveRecipe(string id)
        {
            var recipe = await _recipeManager.GetRecipe(id, GetUserId());
            if (recipe == null)
            {
                return GetNotFoundActionResult();
            }

            return View(new RemoveRecipeViewModel()
            {
                Recipe = recipe
            });
        }

        [HttpPost("{id}/remove")]
        public virtual async Task<IActionResult> RemoveRecipePost(string id)
        {
            var recipe = await _recipeManager.GetRecipe(id, GetUserId());
            if (recipe == null)
            {
                return GetNotFoundActionResult();
            }

            await _recipeManager.RemoveRecipe(id);
            return RedirectToAction("GetRecipes", new
            {
                statusMessage = new StatusMessageModel()
                {
                    Message = $"Recipe {recipe.Name} deleted successfully",
                    Severity = StatusMessageModel.StatusSeverity.Success
                }.ToString()
            });
        }


        [HttpGet("{id}/logs")]
        public virtual async Task<IActionResult> GetRecipeLogs(string id, int skip = 0, int take = 100)
        {
            var recipe = await _recipeManager.GetRecipe(id, GetUserId());
            if (recipe == null)
            {
                return GetNotFoundActionResult();
            }

            var recipeInvocations = await _recipeManager.GetRecipeInvocations(new RecipeInvocationsQuery()
            {
                Skip = skip,
                Take = take,
                RecipeId = id,
                OrderBy = new OrderBy<RecipeInvocationsQuery.RecipeInvocationsQueryOrderBy>()
                {
                    Field = RecipeInvocationsQuery.RecipeInvocationsQueryOrderBy.Timestamp,
                    Direction = OrderDirection.Descending
                }
            });

            return View(new GetRecipeLogsViewModel()
            {
                Name = recipe.Name,
                Id = id,
                Skip = skip,
                Take = take,
                RecipeInvocations = recipeInvocations.ToList()
            });
        }

        [HttpGet("{id}/clone")]
        public virtual async Task<IActionResult> CloneRecipe(string id, string name = null, bool enabled = false)
        {
	        var result = await _recipeManager.CloneRecipe(id, enabled, name);
	        if (result == null)
	        {
		        return RedirectToAction(nameof(GetRecipes), new
		        {
			        statusMessage = new StatusMessageModel()
			        {
				        Message = "Could not clone recipe",
				        Severity = StatusMessageModel.StatusSeverity.Error
			        }.ToString()
		        });
	        }

	        return RedirectToAction(nameof(EditRecipe), new
	        {
		        id = result.Id,
		        statusMessage = new StatusMessageModel()
		        {
			        Message = "Recipe cloned",
			        Severity = StatusMessageModel.StatusSeverity.Success
		        }.ToString()
	        });
        }

        [HttpGet("create")]
        public virtual IActionResult CreateRecipe(string statusMessage)
        {
            return View(new CreateRecipeViewModel()
            {
                StatusMessage = statusMessage
            });
        }

        [HttpPost("create")]
        public virtual async Task<IActionResult> CreateRecipe(CreateRecipeViewModel viewModel)
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
                UserId = GetUserId()
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
        public virtual async Task<IActionResult> EditRecipe(string id, string statusMessage)
        {
            var recipe = await _recipeManager.GetRecipe(id, GetUserId());
            if (recipe == null)
            {
                return GetNotFoundActionResult();
            }

            var orderedGroups = recipe.RecipeActionGroups;
            orderedGroups.ForEach(group =>
            {
                group.RecipeActions = group.RecipeActions.OrderBy(action => action.Order).ToList();
            });
            return View(new EditRecipeViewModel()
            {
                Id = id,
                StatusMessage = statusMessage,
                Name = recipe.Name,
                Enabled = recipe.Enabled,
                Description = recipe.Description,
                Actions = recipe.RecipeActions,
                Trigger = recipe.RecipeTrigger,
                ActionGroups = orderedGroups
            });
        }

        [HttpPost("{id}")]
        public virtual async Task<IActionResult> EditRecipe(string id, EditRecipeViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                return View(viewModel);
            }

            var recipe = await _recipeManager.GetRecipe(id, GetUserId());
            if (recipe == null)
            {
                return GetNotFoundActionResult();
            }

            recipe.Name = viewModel.Name;
            recipe.Enabled = viewModel.Enabled;
            recipe.Description = viewModel.Description;

            await _recipeManager.AddOrUpdateRecipe(recipe);

            return RedirectToAction("EditRecipe", new {id = recipe.Id, statusMessage = "Recipe edited"});
        }

        [HttpGet("{id}/action-groups/add")]
        public virtual async Task<IActionResult> AddRecipeActionGroup(string id)
        {
            var recipe = await _recipeManager.GetRecipe(id, GetUserId());
            if (recipe == null)
            {
                return GetNotFoundActionResult();
            }

            await _recipeManager.AddRecipeActionGroup(new RecipeActionGroup()
            {
                RecipeId = recipe.Id
            });
            return RedirectToAction("EditRecipe", new {id = recipe.Id, statusMessage = "Recipe Action group added"});
        }

        [HttpGet("{id}/action-groups/{actionGroupId}/remove")]
        public virtual async Task<IActionResult> RemoveRecipeActionGroup(string id, string actionGroupId)
        {
            var recipe = await _recipeManager.GetRecipe(id, GetUserId());
            if (recipe == null || !
                    recipe.RecipeActionGroups.Any(group =>
                        group.Id.Equals(actionGroupId, StringComparison.InvariantCultureIgnoreCase)))
            {
                return GetNotFoundActionResult();
            }

            await _recipeManager.RemoveRecipeActionGroup(actionGroupId);
            return RedirectToAction("EditRecipe", new {id = recipe.Id, statusMessage = "Recipe Action group removed"});
        }

        [HttpPost("{id}/action-groups/{actionGroupId}/order")]
        public virtual async Task<IActionResult> ReorderRecipeActionGroup(string id, string actionGroupId,
            UpdateActionGroupOrderViewModel vm)
        {
            var recipe = await _recipeManager.GetRecipe(id, GetUserId());
            if (recipe == null || !
                    recipe.RecipeActionGroups.Any(group =>
                        group.Id.Equals(actionGroupId, StringComparison.InvariantCultureIgnoreCase)))
            {
                return GetNotFoundActionResult();
            }

            await _recipeManager.ReorderRecipeActionGroupActions(actionGroupId,
                vm.UpdateActionGroupOrderItems.ToDictionary(item => item.RecipeActionId, item => item.Order));
            return RedirectToAction("EditRecipe",
                new {id = recipe.Id, statusMessage = "Recipe Action group order updated"});
        }


        private RedirectToActionResult GetNotFoundActionResult()
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

        protected virtual string GetUserId()
        {
            return _userManager.GetUserId(User);
        }
    }
}