using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using BtcTransmuter.Abstractions.Actions;
using BtcTransmuter.Abstractions.Helpers;
using BtcTransmuter.Data.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;

namespace BtcTransmuter.Extension.Webhook.Actions.MakeWebRequest
{
    public class MakeWebRequestActionHandler : BaseActionHandler<MakeWebRequestData, HttpResponseMessage>
    {
        private readonly IHttpClientFactory _httpClientFactory;
        public override string ActionId => "MakeWebRequest";
        public override string Name => "Make web request";

        public override string Description =>
            "Make an Http Web Request";

        public override string ViewPartial => "ViewMakeWebRequestAction";
        public override string ControllerName => "MakeWebRequest";

        public MakeWebRequestActionHandler()
        {
        }

        public MakeWebRequestActionHandler(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public override Task<IActionResult> EditData(RecipeAction data)
        {
            using (var scope = DependencyHelper.ServiceScopeFactory.CreateScope())
            {
                var identifier = $"{Guid.NewGuid()}";
                var memoryCache = scope.ServiceProvider.GetService<IMemoryCache>();
                memoryCache.Set(identifier, data, new MemoryCacheEntryOptions()
                {
                    SlidingExpiration = TimeSpan.FromMinutes(60)
                });

                return Task.FromResult<IActionResult>(new RedirectToActionResult(
                    nameof(MakeWebRequestController.EditData),
                    "MakeWebRequest", new
                    {
                        identifier
                    }));
            }
        }

        protected override async Task<TypedActionHandlerResult<HttpResponseMessage>> Execute(Dictionary<string, object> data, RecipeAction recipeAction,
            MakeWebRequestData actionData)
        {
            try
            {
                using (var client = _httpClientFactory.CreateClient())
                {
                    var result = await client.SendAsync(
                        new HttpRequestMessage(new HttpMethod(actionData.Method),
                            InterpolateString(actionData.Url, data))
                        {
                            Content = new StringContent(InterpolateString(actionData.Body, data) ?? "",
                                Encoding.UTF8, InterpolateString(actionData.ContentType, data))
                        });
                    return new TypedActionHandlerResult<HttpResponseMessage>()
                    {
                        TypedData = result,
                        Executed = true,
                        Result =
                            $"Request sent. Status Code: {result.StatusCode}"
                    };
                }
            }
            catch (Exception e)
            {
                return new TypedActionHandlerResult<HttpResponseMessage>()
                {
                    Executed = true,
                    Result =
                        $"Could not make web request because {e.Message}"
                };
            }
        }
    }
}