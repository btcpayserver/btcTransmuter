using System;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using BtcTransmuter.Abstractions.Helpers;
using BtcTransmuter.Data.Entities;
using BtcTransmuter.Data.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;

namespace BtcTransmuter.Abstractions.Actions
{
    public abstract class BaseActionHandler<TActionData> : IActionHandler, IActionDescriptor
    {
        public abstract string ActionId { get; }
        public abstract string Name { get; }
        public abstract string Description { get; }
        public abstract string ViewPartial { get; }

        public abstract string ControllerName { get; }

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

        protected abstract Task<bool> CanExecute(object triggerData, RecipeAction recipeAction);

        public async Task<ActionHandlerResult> Execute(object triggerData, RecipeAction recipeAction)
        {
            if (await CanExecute(triggerData, recipeAction))
            {
                return await Execute(triggerData, recipeAction, recipeAction.Get<TActionData>());
            }

            return new ActionHandlerResult()
            {
                Executed = false
            };
        }

        protected abstract Task<ActionHandlerResult> Execute(object triggerData, RecipeAction recipeAction,
            TActionData actionData);

        /// <summary>
        /// https://dotnetfiddle.net/MoqJFk
        /// </summary>
        protected static string InterpolateString(string value, object @object)
        {
            try
            {
                return Regex.Replace(value, @"{{(.+?)}}",
                    match =>
                    {
                        var p = Expression.Parameter(@object.GetType(), "TriggerData");
                        var e = System.Linq.Dynamic.DynamicExpression.ParseLambda(new[] {p}, null,
                            match.Groups[1].Value);
                        return (e.Compile().DynamicInvoke(@object) ?? "").ToString();
                    });
            }
            catch (Exception e)
            {
                return value;
            }
        }
    }
}