using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BtcTransmuter.Abstractions.Recipes;
using BtcTransmuter.Abstractions.Triggers;
using BtcTransmuter.Extension.Timer.Triggers.Timer;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace BtcTransmuter.Extension.Timer.HostedServices
{
    public class TimerHostedService : IHostedService
    {
        private readonly ITriggerDispatcher _triggerDispatcher;
        private readonly ILogger<TimerHostedService> _logger;

        public TimerHostedService(ITriggerDispatcher triggerDispatcher, ILogger<TimerHostedService> logger)
        {
            _triggerDispatcher = triggerDispatcher;
            _logger = logger;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Starting Timer Service");
            _ = Loop(cancellationToken);
            return Task.CompletedTask;
        }

        private async Task Loop(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                _ =  _triggerDispatcher.DispatchTrigger(new TimerTrigger());
                await Task.Delay(TimeSpan.FromMinutes(1), cancellationToken);
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}