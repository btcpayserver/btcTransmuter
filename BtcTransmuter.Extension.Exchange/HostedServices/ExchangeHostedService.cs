using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BtcTransmuter.Abstractions.ExternalServices;
using BtcTransmuter.Abstractions.Recipes;
using BtcTransmuter.Abstractions.Triggers;
using BtcTransmuter.Extension.Exchange.ExternalServices.Exchange;
using BtcTransmuter.Extension.Exchange.Triggers.CheckExchangeBalance;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace BtcTransmuter.Extension.NBXplorer.HostedServices
{
    public class ExchangeHostedService : IHostedService
    {
        private readonly IExternalServiceManager _externalServiceManager;
        private readonly ILogger<ExchangeHostedService> _logger;
        private readonly ITriggerDispatcher _triggerDispatcher;

        public ExchangeHostedService(
            IExternalServiceManager externalServiceManager,
            ILogger<ExchangeHostedService> logger,
            ITriggerDispatcher triggerDispatcher)
        {
            _externalServiceManager = externalServiceManager;
            _logger = logger;
            _triggerDispatcher = triggerDispatcher;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _ = UpdateBalances_Continuously(cancellationToken);
            return Task.CompletedTask;
        }

        private async Task UpdateBalances_Continuously(
            CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var exchangeExternalServices = await
                    _externalServiceManager.GetExternalServicesData(new ExternalServicesDataQuery()
                    {
                        Type = new string[]
                        {
                            ExchangeService.ExchangeServiceType
                        }
                    });


                await Task.WhenAll(exchangeExternalServices.Select(async data =>
                {
                    var exchangeService = new ExchangeService(data);
                    var client = exchangeService.ConstructClient();
                    var amounts = await client.GetAmountsAsync();
                    foreach (var keyValuePair in amounts)
                    {
                        _ = _triggerDispatcher.DispatchTrigger(new CheckExchangeBalanceTrigger()
                        {
                            Data = new CheckExchangeBalanceTriggerData()
                            {
                                Balance = keyValuePair.Value,
                                Asset = keyValuePair.Key,
                                ExternalServiceId = data.Id
                            }
                        });
                    }
                }));
                await Task.Delay(TimeSpan.FromMinutes(1),
                    cancellationToken);
            }
        }


   
        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}