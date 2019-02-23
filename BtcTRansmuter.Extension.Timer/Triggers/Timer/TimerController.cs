using System.Threading.Tasks;
using BtcTransmuter.Abstractions.Recipes;
using BtcTransmuter.Data.Entities;
using BtcTransmuter.Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace BtcTransmuter.Extension.Timer.Triggers.Timer
{
    [Authorize]
    [Route("timer-plugin/triggers/timer")]
    public class TimerController : Controller
    {
        

        private readonly IRecipeManager _recipeManager;
        private readonly UserManager<User> _userManager;
        private readonly IMemoryCache _memoryCache;


        public TimerController(
            IRecipeManager recipeManager,
            UserManager<User> userManager,
            IMemoryCache memoryCache)
        {
            _recipeManager = recipeManager;
            _userManager = userManager;
            _memoryCache = memoryCache;
        }

        [HttpGet("{identifier}")]
        public async Task<IActionResult> EditData(string identifier)
        {
            var result = await GetRecipeTrigger(identifier);
            if (result.Error != null)
            {
                return result.Error;
            }

            var vm = new TimerTriggerViewModel(result.Data.Get<TimerTriggerParameters>(), result.Data.RecipeId);

            return View(vm);
        }

        [HttpPost("{identifier}")]
        public async Task<IActionResult> EditData(string identifier, TimerTriggerViewModel data)
        {
            var result = await GetRecipeTrigger(identifier);
            if (result.Error != null)
            {
                return result.Error;
            }

            if (data.TriggerEveryAmount <= 0)
            {
                ModelState.AddModelError(nameof(TimerTriggerViewModel.TriggerEveryAmount), "Amount needs to be at least 1");
            }

            if (!ModelState.IsValid)
            {
                return View(data);
            }

            var recipeTrigger = result.Data;
            recipeTrigger.Set(data);

            await _recipeManager.AddOrUpdateRecipeTrigger(recipeTrigger);
            return RedirectToAction("EditRecipe", "Recipes", new
            {
                id = recipeTrigger.RecipeId,
                statusMessage = "Invoice Timer trigger Updated"
            });
        }

        private async Task<(IActionResult Error, RecipeTrigger Data )> GetRecipeTrigger(string identifier)
        {
            if (!_memoryCache.TryGetValue(identifier, out RecipeTrigger data))
            {
                return (RedirectToAction("GetRecipes", "Recipes", new
                {
                    statusMessage = "Error:Data could not be found or data session expired"
                }), null);
            }

            var recipe = await _recipeManager.GetRecipe(data.RecipeId, _userManager.GetUserId(User));

            if (recipe == null)
            {
                return (RedirectToAction("GetRecipes", "Recipes", new
                {
                    statusMessage = "Error:Data could not be found or data session expired"
                }), null);
            }

            return (null, data);
        }


        public class TimerTriggerViewModel : TimerTriggerParameters
        {
            public TimerTriggerViewModel()
            {
                
            }
            public TimerTriggerViewModel(TimerTriggerParameters data, string recipeId)
            {
                StartOn = data.StartOn;
                TriggerEvery = data.TriggerEvery;
                TriggerEveryAmount= data.TriggerEveryAmount;
                RecipeId = recipeId;
            }
            public string RecipeId { get; private set; }
        }
    }
}