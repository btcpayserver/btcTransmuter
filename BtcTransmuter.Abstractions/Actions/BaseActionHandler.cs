using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using BtcTransmuter.Abstractions.Helpers;
using BtcTransmuter.Data.Entities;
using BtcTransmuter.Data.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;

namespace BtcTransmuter.Abstractions.Actions
{
    public abstract class BaseActionHandler<TActionData, TActionResultData> : IActionHandler, IActionDescriptor
    {
        public abstract string ActionId { get; }
        public abstract string Name { get; }
        public abstract string Description { get; }
        public abstract string ViewPartial { get; }

        public abstract string ControllerName { get; }
        
        public Type ActionResultDataType => typeof(TActionResultData);

        public virtual Task<IActionResult> EditData(RecipeAction data)
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
                    "EditData",
                    ControllerName, new
                    {
                        identifier
                    }));
            }
        }

        public virtual Task<bool> CanExecute(Dictionary<string,  (object data, string json)> data, RecipeAction recipeAction)
        {
            return Task.FromResult(recipeAction.ActionId == ActionId);
        }

        public Task<ActionHandlerResult> Execute(Dictionary<string, (object data, string json)> data, RecipeAction recipeAction)
        {
            return Execute(data.ToDictionary(pair => pair.Key, pair => pair.Value.data), recipeAction);
        }
        public async Task<ActionHandlerResult> Execute(Dictionary<string, object> data, RecipeAction recipeAction)
        {
            return await Execute(data, recipeAction, recipeAction.Get<TActionData>());
        }

        protected abstract Task<TypedActionHandlerResult<TActionResultData>> Execute(Dictionary<string, object> data, RecipeAction recipeAction,
            TActionData actionData);

        /// <summary>
        /// https://dotnetfiddle.net/MoqJFk
        /// </summary>
        protected static string InterpolateString(string value, Dictionary<string, object> data)
        {
            return InterpolationHelper.InterpolateString(value, data);
        }
    }
}