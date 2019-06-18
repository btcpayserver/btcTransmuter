using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BtcTransmuter.Abstractions.ExternalServices;
using BtcTransmuter.Abstractions.Triggers;
using BtcTransmuter.Data.Entities;
using BtcTransmuter.Extension.Lightning.ExternalServices.LightningNode;
using BtcTransmuter.Extension.Lightning.Triggers.ReceivedLightningPayment;
using BtcTransmuter.Extension.NBXplorer.Services;
using Microsoft.Extensions.Hosting;
using NBitcoin;

namespace BtcTransmuter.Extension.Lightning.HostedServices
{
    public class LightningPaymentWatcherHostedService : IHostedService
    {
        private readonly IExternalServiceManager _externalServiceManager;
        private readonly ITriggerDispatcher _triggerDispatcher;
        private readonly NBXplorerClientProvider _nbXplorerClientProvider;
        private readonly NBXplorerSummaryProvider _nbXplorerSummaryProvider;
        private readonly SocketFactory _socketFactory;
        private ConcurrentDictionary<string, LightningNodeService> _externalServices;
        private ConcurrentDictionary<string, CancellationTokenSource> _cancellationTokens = new ConcurrentDictionary<string, CancellationTokenSource>();
        private CancellationToken _cts;

        public LightningPaymentWatcherHostedService(IExternalServiceManager externalServiceManager,
            ITriggerDispatcher triggerDispatcher, NBXplorerClientProvider nbXplorerClientProvider,
            NBXplorerSummaryProvider nbXplorerSummaryProvider, SocketFactory socketFactory)
        {
            _externalServiceManager = externalServiceManager;
            _triggerDispatcher = triggerDispatcher;
            _nbXplorerClientProvider = nbXplorerClientProvider;
            _nbXplorerSummaryProvider = nbXplorerSummaryProvider;
            _socketFactory = socketFactory;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _cts = cancellationToken;
            var services = await _externalServiceManager.GetExternalServicesData(new ExternalServicesDataQuery()
            {
                Type = new[]
                {
                    LightningNodeService.LightningNodeServiceType
                }
            });
            _externalServices = new ConcurrentDictionary<string, LightningNodeService>(
                services
                    .Select(service =>
                        new KeyValuePair<string, LightningNodeService>(service.Id,
                            new LightningNodeService(service, _nbXplorerClientProvider, _nbXplorerSummaryProvider,
                                _socketFactory))));

            _externalServiceManager.ExternalServiceDataUpdated += ExternalServiceManagerOnExternalServiceUpdated;
            foreach (var lightningNodeService in _externalServices)
            {
                _ = ListenForPayment(cancellationToken, lightningNodeService);
            };
        }

        private async Task ListenForPayment(CancellationToken cancellationToken, KeyValuePair<string, LightningNodeService> lightningNodeService)
        {
            var linkedCancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            _cancellationTokens.AddOrReplace(lightningNodeService.Key, linkedCancellation);
            _ = (await lightningNodeService.Value.ConstructClient().Listen(linkedCancellation.Token))
                .WaitInvoice(linkedCancellation.Token).ContinueWith(
                    task =>
                    {
                        _ = _triggerDispatcher.DispatchTrigger(new ReceivedLightningPaymentTrigger()
                        {
                            Data = new ReceivedLightningPaymentTriggerData()
                            {
                                LightningInvoice = task.Result,
                                ExternalServiceId = lightningNodeService.Key
                            }
                        });
                        _ = ListenForPayment(cancellationToken, lightningNodeService);
                    }, TaskScheduler.Default);
        }

      

        public Task StopAsync(CancellationToken cancellationToken)
        {
            foreach (var cancellationTokensValue in _cancellationTokens.Values)
            {
                cancellationTokensValue.Cancel();
            }

            return Task.CompletedTask;
        }

        private void ExternalServiceManagerOnExternalServiceUpdated(object sender, UpdatedItem<ExternalServiceData> e)
        {
            if (e.Item.Type != LightningNodeService.LightningNodeServiceType)
            {
                return;
            }
            if (_cancellationTokens.ContainsKey(e.Item.Id))
            {
                _cancellationTokens[e.Item.Id].Cancel();
            }
            switch (e.Action)
            {
                case UpdatedItem<ExternalServiceData>.UpdateAction.Added:
                    _externalServices.TryAdd(e.Item.Id,
                        new LightningNodeService(e.Item, _nbXplorerClientProvider, _nbXplorerSummaryProvider,
                            _socketFactory));
                    break;
                case UpdatedItem<ExternalServiceData>.UpdateAction.Removed:
                    _externalServices.TryRemove(e.Item.Id, out var _);
                    break;
                case UpdatedItem<ExternalServiceData>.UpdateAction.Updated:
                    _externalServices.TryUpdate(e.Item.Id, new LightningNodeService(e.Item, _nbXplorerClientProvider,
                        _nbXplorerSummaryProvider,
                        _socketFactory), null);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (_externalServices.ContainsKey(e.Item.Id))
            {
                _ =ListenForPayment(_cts, _externalServices.Single(pair => pair.Key.Equals(e.Item.Id)));
            }
            
        }
    }
}