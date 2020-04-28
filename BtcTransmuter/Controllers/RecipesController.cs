using System;
using System.Linq;
using System.Threading.Tasks;
using BtcTransmuter.Abstractions.Extensions;
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
        public virtual async Task<IActionResult> GetRecipes([FromQuery] string statusMessage = null, GetRecipesViewModel.ListMode mode = GetRecipesViewModel.ListMode.Cards)
        {
            var recipes = await _recipeManager.GetRecipes(new RecipesQuery()
            {
                UserId = GetUserId()
            });
            return this.ViewOrJson("GetRecipes", new GetRecipesViewModel()
            {
                StatusMessage = statusMessage,
                Recipes = recipes,
                ViewMode = mode
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
            if (this.IsApi())
            {
                return Ok();
            }
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

            return this.ViewOrJson(new GetRecipeLogsViewModel()
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
                if (this.IsApi())
                {
                    return BadRequest();
                }
		        return RedirectToAction(nameof(GetRecipes), new
		        {
			        statusMessage = new StatusMessageModel()
			        {
				        Message = "Could not clone recipe",
				        Severity = StatusMessageModel.StatusSeverity.Error
			        }.ToString()
		        });
	        }

            if (this.IsApi())
            {
                return await EditRecipe(result.Id, (string)null);
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
                return this.ViewOrBadRequest(viewModel);
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
                return this.ViewOrBadRequest(viewModel);
            }

            if (this.IsApi())
            {
                return await EditRecipe(recipe.Id, (string) null);
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
            return this.ViewOrJson(new EditRecipeViewModel()
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

            if (this.IsApi())
            {
                return await EditRecipe(recipe.Id, (string) null);
            }
            return RedirectToAction("EditRecipe", new {id = recipe.Id, statusMessage = "Recipe edited"});
        }

        [HttpPost("{id}/action-groups/add")]
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
            if (this.IsApi())
            {
                return await EditRecipe(recipe.Id, (string) null);
            }
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
            if (this.IsApi())
            {
                return await EditRecipe(recipe.Id, (string) null);
            }
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
                vm.UpdateActionGroupOrderItems?.ToDictionary(item => item.RecipeActionId, item => item.Order));
            if (this.IsApi())
            {
                return await EditRecipe(recipe.Id, (string) null);
            }
            return RedirectToAction("EditRecipe",
                new {id = recipe.Id, statusMessage = "Recipe Action group order updated"});
        }


        private ActionResult GetNotFoundActionResult()
        {
            if (this.IsApi())
            {
                return NotFound();
            }
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