using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using BtcTransmuter.Abstractions.Actions;
using BtcTransmuter.Abstractions.ExternalServices;
using BtcTransmuter.Abstractions.Helpers;
using BtcTransmuter.Data.Entities;
using BtcTransmuter.Data.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;

namespace BtcTransmuter.Abstractions.Triggers
{
    public abstract class BaseTriggerHandler<TTriggerData, TTriggerParameters> : ITriggerHandler, ITriggerDescriptor
        where TTriggerParameters : class
    {
        public abstract string TriggerId { get; }
        public abstract string Name { get; }
        public abstract string Description { get; }
        public abstract string ViewPartial { get; }
        public abstract string ControllerName { get; }

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
                    "EditData",
                    ControllerName, new
                    {
                        identifier
                    }));
            }
        }

        protected abstract Task<bool> IsTriggered(ITrigger trigger, RecipeTrigger recipeTrigger,
            TTriggerData triggerData, TTriggerParameters parameters);


        public async Task<bool> IsTriggered(ITrigger trigger, RecipeTrigger recipeTrigger)
        {
            if (recipeTrigger.TriggerId != trigger.Id || trigger.Id != TriggerId)
            {
                return false;
            }

            var triggerData = await GetTriggerData(trigger);

            if (typeof(IUseExternalService).IsAssignableFrom(typeof(TTriggerData)) &&
                ((IUseExternalService) triggerData).ExternalServiceId != recipeTrigger.ExternalServiceId)
            {
                return false;
            }

            return await IsTriggered(trigger, recipeTrigger, triggerData, recipeTrigger.Get<TTriggerParameters>());
        }

        public async Task<object> GetData(ITrigger trigger)
        {
            return await GetTriggerData(trigger);
        }

        public virtual Task<TTriggerData> GetTriggerData(ITrigger trigger)
        {
            return Task.FromResult(trigger.Get<TTriggerData>());
        }

        public virtual Task AfterExecution(IEnumerable<Recipe> tupleItem1)
        {
            return Task.CompletedTask;
        }
    }
}