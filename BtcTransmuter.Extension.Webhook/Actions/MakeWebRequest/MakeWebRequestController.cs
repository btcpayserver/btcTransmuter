using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using BtcTransmuter.Abstractions.Actions;
using BtcTransmuter.Abstractions.ExternalServices;
using BtcTransmuter.Abstractions.Recipes;
using BtcTransmuter.Data.Entities;
using BtcTransmuter.Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace BtcTransmuter.Extension.Webhook.Actions.MakeWebRequest
{
    [Route("webhook-plugin/actions/[controller]")]
    [Authorize]
    public class MakeWebRequestController : BaseActionController<MakeWebRequestController.MakeWebRequestViewModel,
        MakeWebRequestData>
    {
        public static readonly List<string> AllowedMethods = new List<string>()
        {
            HttpMethod.Get.ToString(),
            HttpMethod.Put.ToString(),
            HttpMethod.Head.ToString(),
            HttpMethod.Post.ToString(),
            "Patch", //HttpMethod.Patch.ToString(),
            HttpMethod.Trace.ToString(),
            HttpMethod.Delete.ToString(),
            HttpMethod.Options.ToString()
        };

        public static IEnumerable<string> ContentTypes = new List<string>()
        {
            "application/json",
            "text/plain",
        };

        public MakeWebRequestController(IMemoryCache memoryCache, UserManager<User> userManager,
            IRecipeManager recipeManager, IExternalServiceManager externalServiceManager) : base(memoryCache,
            userManager, recipeManager, externalServiceManager)
        {
        }

        protected override Task<MakeWebRequestViewModel> BuildViewModel(RecipeAction recipeAction)
        {
            var data = recipeAction.Get<MakeWebRequestData>();
            return Task.FromResult(new MakeWebRequestViewModel
            {
                Url = data.Url,
                Body = data.Body,
                Method = data.Method,
                ContentType = data.ContentType,
                RecipeId = recipeAction.RecipeId
            });
        }

        protected override Task<(RecipeAction ToSave, MakeWebRequestViewModel showViewModel)> BuildModel(
            MakeWebRequestViewModel viewModel, RecipeAction mainModel)
        {
            if (ModelState.IsValid)
            {
                mainModel.Set<MakeWebRequestData>(viewModel);
                return Task.FromResult<(RecipeAction ToSave, MakeWebRequestViewModel showViewModel)>((mainModel, null));
            }

            return Task.FromResult<(RecipeAction ToSave, MakeWebRequestViewModel showViewModel)>((null, viewModel));
        }


        public class MakeWebRequestViewModel : MakeWebRequestData, IActionViewModel
        {
            public string RecipeId { get; set; }
            public string RecipeActionIdInGroupBeforeThisOne { get; set; }
        }
    }
}