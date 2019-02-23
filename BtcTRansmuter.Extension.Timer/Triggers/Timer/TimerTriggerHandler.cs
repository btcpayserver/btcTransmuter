using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BtcTransmuter.Abstractions.Helpers;
using BtcTransmuter.Abstractions.Recipes;
using BtcTransmuter.Abstractions.Triggers;
using BtcTransmuter.Data.Entities;
using BtcTransmuter.Data.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;

namespace BtcTransmuter.Extension.Timer.Triggers.Timer
{
    public class TimerTriggerHandler : BaseTriggerHandler<TimerTriggerData,
        TimerTriggerParameters>, ITriggerDescriptor
    {
        public override string TriggerId => new TimerTrigger().Id;
        public string Name => "Timer";

        public string Description =>
            "Trigger a recipe every X time";

        public string ViewPartial => "ViewTimerTrigger";
        public Task<IActionResult> EditData(RecipeTrigger data)
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
                    nameof(TimerController.EditData),
                    "Timer", new
                    {
                        identifier
                    }));
            }
        }


        protected override Task<bool> IsTriggered(ITrigger trigger, RecipeTrigger recipeTrigger,
            TimerTriggerData triggerData,
            TimerTriggerParameters parameters)
        {
            if (parameters.StartOn.HasValue && parameters.StartOn < DateTime.Now)
            {
                return Task.FromResult(false);
            }

            if (!parameters.LastTriggered.HasValue)
            {
                return Task.FromResult(true);
            }

            switch (parameters.TriggerEvery)
            {
                case TimerTriggerParameters.TimerResetEvery.Minute:
                    return Task.FromResult(parameters.LastTriggered.Value.Subtract(DateTime.Now).TotalMinutes >=
                                           parameters.TriggerEveryAmount);
                case TimerTriggerParameters.TimerResetEvery.Hour:
                    return Task.FromResult(parameters.LastTriggered.Value.Subtract(DateTime.Now).TotalHours >=
                                           parameters.TriggerEveryAmount);
                case TimerTriggerParameters.TimerResetEvery.Day:
                    return Task.FromResult(parameters.LastTriggered.Value.Subtract(DateTime.Now).TotalDays >=
                                           parameters.TriggerEveryAmount);
                case TimerTriggerParameters.TimerResetEvery.Month:
                    return Task.FromResult(parameters.LastTriggered.Value.MonthDifference(DateTime.Now) >=
                                           parameters.TriggerEveryAmount);
                case TimerTriggerParameters.TimerResetEvery.Year:
                    return Task.FromResult(parameters.LastTriggered.Value.YearDifference(DateTime.Now) >=
                                           parameters.TriggerEveryAmount);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public override Task AfterExecution(IEnumerable<Recipe> recipes)
        {
            using (var scope = DependencyHelper.ServiceScopeFactory.CreateScope())
            {
                var recipeManager = scope.ServiceProvider.GetService<IRecipeManager>();
                var triggers = recipes.Select(x => x.RecipeTrigger).ToList();
                foreach (var recipeTrigger in triggers)
                {
                    var currentParams = recipeTrigger.Get<TimerTriggerParameters>();

                    currentParams.LastTriggered = DateTime.Now;
                    recipeTrigger.Set(currentParams);
                }

                return recipeManager.AddOrUpdateRecipeTriggers(triggers);
            }
        }
    }
}