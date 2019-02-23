using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BtcTransmuter.Abstractions.Recipes;
using BtcTransmuter.Abstractions.Triggers;
using BtcTransmuter.Extension.Timer.Triggers.Timer;
using Microsoft.Extensions.Hosting;

namespace BtcTransmuter.Extension.Timer.HostedServices
{
    public class TimerHostedService : IHostedService
    {
        private readonly ITriggerDispatcher _triggerDispatcher;
        private readonly IRecipeManager _recipeManager;

        public TimerHostedService(ITriggerDispatcher triggerDispatcher, IRecipeManager recipeManager)
        {
            _triggerDispatcher = triggerDispatcher;
            _recipeManager = recipeManager;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _ = Loop(cancellationToken);
            return Task.CompletedTask;
        }

        private async Task Loop(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                await _triggerDispatcher.DispatchTrigger(new TimerTrigger());
                await Task.Delay(TimeSpan.FromMinutes(1), cancellationToken);
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}