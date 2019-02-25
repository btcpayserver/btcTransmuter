using System.Collections;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using BtcTransmuter.Abstractions.Recipes;
using BtcTransmuter.Data.Entities;
using BtcTransmuter.Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Caching.Memory;

namespace BtcTransmuter.Extension.Webhook.Actions.MakeWebRequest
{
    [Route("webhook-plugin/actions/make-web-request")]
    [Authorize]
    public class MakeWebRequestController : Controller
    {
        private readonly IRecipeManager _recipeManager;
        private readonly UserManager<User> _userManager;
        private readonly IMemoryCache _memoryCache;

        public MakeWebRequestController(
            IRecipeManager recipeManager,
            UserManager<User> userManager,
            IMemoryCache memoryCache)
        {
            _recipeManager = recipeManager;
            _userManager = userManager;
            _memoryCache = memoryCache;
        }

        public static readonly List<string> AllowedMethods = new List<string>()
        {
            HttpMethod.Get.ToString(),
            HttpMethod.Put.ToString(),
            HttpMethod.Head.ToString(),
            HttpMethod.Post.ToString(),
            HttpMethod.Patch.ToString(),
            HttpMethod.Trace.ToString(),
            HttpMethod.Delete.ToString(),
            HttpMethod.Options.ToString()
        };

        public static IEnumerable<string> ContentTypes = new List<string>()
        {
            "application/json",
            "text/plain",
        };


        [HttpGet("{identifier}")]
        public async Task<IActionResult> EditData(string identifier)
        {
            var result = await GetRecipeAction(identifier);
            if (result.Error != null)
            {
                return result.Error;
            }
            var vm = new MakeWebRequestViewModel()
            {
                RecipeId = result.Data.RecipeId
            };
            await SetValues(result.Data, vm);

            return View(vm);
        }

        private void SetValues(MakeWebRequestViewModel from, RecipeAction to)
        {
            to.RecipeId = from.RecipeId;
            to.Set((MakeWebRequestData) from);
        }

        private async Task SetValues(RecipeAction from, MakeWebRequestViewModel to)
        {
            var data = from.Get<MakeWebRequestData>();
            to.Url = data.Url;
            to.Body = data.Body;
            to.Method = data.Method;
            to.ContentType= data.ContentType;
            to.RecipeId = from.RecipeId;
            
        }

        [HttpPost("{identifier}")]
        public async Task<IActionResult> EditData(string identifier, MakeWebRequestViewModel data)
        {
            var result = await GetRecipeAction(identifier);
            if (result.Error != null)
            {
                return result.Error;
            }

            if (!ModelState.IsValid)
            {
                return View(data);
            }

            var recipeAction = result.Data;
            SetValues(data, recipeAction);

            await _recipeManager.AddOrUpdateRecipeAction(recipeAction);
            return RedirectToAction("EditRecipe", "Recipes", new
            {
                id = recipeAction.RecipeId,
                statusMessage = "Make Web Request Action Updated"
            });
        }

        private async Task<(IActionResult Error, RecipeAction Data )> GetRecipeAction(string identifier)
        {
            if (!_memoryCache.TryGetValue(identifier, out RecipeAction data))
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


        public class MakeWebRequestViewModel : MakeWebRequestData
        { 
            public string RecipeId { get; set; }
        }
    }
}