using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using BtcTransmuter.Abstractions.ExternalServices;
using BtcTransmuter.Abstractions.Recipes;
using BtcTransmuter.Abstractions.Triggers;
using BtcTransmuter.Data.Entities;
using BtcTransmuter.Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace BtcTransmuter.Extension.Webhook.Triggers.ReceiveWebRequest
{
    [Authorize]
    [Route("webhook-plugin/triggers/[controller]")]
    public class ReceiveWebRequestController : BaseTriggerController<
        ReceiveWebRequestController.ReceiveWebRequestTriggerViewModel, ReceiveWebRequestTriggerParameters>
    {
        private readonly ITriggerDispatcher _triggerDispatcher;

        public static readonly List<string> AllowedMethods = new List<string>()
        {
            "",
            HttpMethod.Get.ToString(),
            HttpMethod.Put.ToString(),
            HttpMethod.Head.ToString(),
            HttpMethod.Post.ToString(),
            "Patch", //HttpMethod.Patch.ToString(),
            HttpMethod.Trace.ToString(),
            HttpMethod.Delete.ToString(),
            HttpMethod.Options.ToString()
        };


        public ReceiveWebRequestController(IRecipeManager recipeManager, UserManager<User> userManager,
            IMemoryCache memoryCache, ITriggerDispatcher triggerDispatcher,
            IExternalServiceManager externalServiceManager) : base(recipeManager, userManager, memoryCache,
            externalServiceManager)
        {
            _triggerDispatcher = triggerDispatcher;
        }

        protected override Task<ReceiveWebRequestTriggerViewModel> BuildViewModel(RecipeTrigger data)
        {
            return Task.FromResult(new ReceiveWebRequestTriggerViewModel(data.Get<ReceiveWebRequestTriggerParameters>(),
                data.RecipeId));
        }

        protected override Task<(RecipeTrigger ToSave, ReceiveWebRequestTriggerViewModel showViewModel)> BuildModel(
            ReceiveWebRequestTriggerViewModel viewModel, RecipeTrigger mainModel)
        {
            if (!string.IsNullOrEmpty(viewModel.Body) &&
                viewModel.BodyComparer == ReceiveWebRequestTriggerParameters.FieldComparer.None)
            {
                ModelState.AddModelError(nameof(ReceiveWebRequestTriggerViewModel.BodyComparer),
                    "Please choose a comparison type or leave field blank");
            }

            if (!ModelState.IsValid)
            {
                return Task.FromResult<(RecipeTrigger ToSave, ReceiveWebRequestTriggerViewModel showViewModel)>((null,
                    viewModel));
            }

            mainModel.Set((ReceiveWebRequestTriggerParameters) viewModel);

            return Task.FromResult<(RecipeTrigger ToSave, ReceiveWebRequestTriggerViewModel showViewModel)>((mainModel,
                null));
        }

        [Route("trigger/{relativeUrl?}")]
        [AllowAnonymous]
        public async Task<IActionResult> Trigger(string relativeUrl)
        {
            string body = null;
            dynamic bodyJson = null;
            try
            {
                using (var stream = new StreamReader(Request.Body))
                {
                    body = await stream.ReadToEndAsync();
                    bodyJson = JsonConvert.DeserializeObject(body);
                }
            }
            catch (Exception)
            {
                // ignored
            }

            _ =  _triggerDispatcher.DispatchTrigger(new ReceiveWebRequestTrigger()
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