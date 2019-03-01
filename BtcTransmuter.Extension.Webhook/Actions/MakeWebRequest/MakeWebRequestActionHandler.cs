using System;
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
    public class MakeWebRequestActionHandler : BaseActionHandler<MakeWebRequestData>
    {
        public override string ActionId => "MakeWebRequest";
        public override string Name => "Make web request";

        public override string Description =>
            "Make an Http Web Request";

        public override string ViewPartial => "ViewMakeWebRequestAction";
        public override string ControllerName => "MakeWebRequest";

        public Task<IActionResult> EditData(RecipeAction data)
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

        protected override Task<bool> CanExecute(object triggerData, RecipeAction recipeAction)
        {
            return Task.FromResult(recipeAction.ActionId == ActionId);
        }

        protected override async Task<ActionHandlerResult> Execute(object triggerData, RecipeAction recipeAction,
            MakeWebRequestData actionData)
        {
            try
            {
                var client = new HttpClient();
                var result = await client.SendAsync(
                    new HttpRequestMessage(new HttpMethod(actionData.Method), InterpolateString(actionData.Url, triggerData))
                    {
                        Content = new StringContent(InterpolateString(actionData.Body, triggerData) ?? "",
                            Encoding.UTF8, InterpolateString(actionData.ContentType, triggerData))
                    });
                return new ActionHandlerResult()
                {
                    Executed = true,
                    Result =
                        $"Request sent. Status Code: {result.StatusCode}"
                };
            }
            catch (Exception e)
            {
                return new ActionHandlerResult()
                {
                    Executed = true,
                    Result =
                        $"Could not make web request because {e.Message}"
                };
            }
        }
    }
}