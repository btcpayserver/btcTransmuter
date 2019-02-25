using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using BtcTransmuter.Abstractions.Recipes;
using BtcTransmuter.Abstractions.Triggers;
using BtcTransmuter.Data.Entities;
using BtcTransmuter.Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json.Linq;

namespace BtcTransmuter.Extension.Webhook.Triggers.ReceiveWebRequest
{
    [Authorize]
    [Route("webhook-plugin/triggers/receive-web-request")]
    public class ReceiveWebRequestController : Controller
    {
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

        private readonly IRecipeManager _recipeManager;
        private readonly UserManager<User> _userManager;
        private readonly IMemoryCache _memoryCache;
        private readonly ITriggerDispatcher _triggerDispatcher;


        public ReceiveWebRequestController(
            IRecipeManager recipeManager,
            UserManager<User> userManager,
            IMemoryCache memoryCache,
            ITriggerDispatcher triggerDispatcher)
        {
            _recipeManager = recipeManager;
            _userManager = userManager;
            _memoryCache = memoryCache;
            _triggerDispatcher = triggerDispatcher;
        }

        [HttpGet("{identifier}")]
        public async Task<IActionResult> EditData(string identifier)
        {
            var result = await GetRecipeTrigger(identifier);
            if (result.Error != null)
            {
                return result.Error;
            }

            var vm = new ReceiveWebRequestTriggerViewModel(result.Data.Get<ReceiveWebRequestTriggerParameters>(),
                result.Data.RecipeId);
            return View(vm);
        }

        [HttpPost("{identifier}")]
        public async Task<IActionResult> EditData(string identifier, ReceiveWebRequestTriggerViewModel data)
        {
            var result = await GetRecipeTrigger(identifier);
            if (result.Error != null)
            {
                return result.Error;
            }

            if (!string.IsNullOrEmpty(data.Body) &&
                data.BodyComparer == ReceiveWebRequestTriggerParameters.FieldComparer.None)
            {
                ModelState.AddModelError(nameof(ReceiveWebRequestTriggerViewModel.BodyComparer),
                    "Please choose a comparison type or leave field blank");
            }

            if (!ModelState.IsValid)
            {
                return View(data);
            }

            var recipeTrigger = result.Data;
            recipeTrigger.Set((ReceiveWebRequestTriggerParameters) data);

            await _recipeManager.AddOrUpdateRecipeTrigger(recipeTrigger);
            return RedirectToAction("EditRecipe", "Recipes", new
            {
                id = recipeTrigger.RecipeId,
                statusMessage = "Receive Web Request trigger Updated"
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

        [Route("trigger/{relativeUrl?}")]
        public async Task<IActionResult> Trigger(string relativeUrl)
        {
            string body = null;
            dynamic bodyJson = null;
            try
            {
                using (var stream = new StreamReader(Request.Body))
                {
                    body = await stream.ReadToEndAsync();
                    bodyJson = JObject.Parse(body);
                }
            }
            catch (Exception)
            {
                // ignored
            }

            await _triggerDispatcher.DispatchTrigger(new ReceiveWebRequestTrigger()
            {
                Data = new ReceiveWebRequestTriggerData()
                {
                    Method = Request.Method,
                    RelativeUrl = relativeUrl,
                    Body = body,
                    BodyJson = bodyJson
                }
            });
            return Ok();
        }


        public class ReceiveWebRequestTriggerViewModel : ReceiveWebRequestTriggerParameters
        {
            public ReceiveWebRequestTriggerViewModel()
            {
            }

            public ReceiveWebRequestTriggerViewModel(ReceiveWebRequestTriggerParameters data, string recipeId)
            {
                Method = data.Method;
                RelativeUrl = data.RelativeUrl;
                Body = data.Body;
                BodyComparer = data.BodyComparer;
                RecipeId = recipeId;
            }

            public string RecipeId { get; private set; }
        }
    }
}