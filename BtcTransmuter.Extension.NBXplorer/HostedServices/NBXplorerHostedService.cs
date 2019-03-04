using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BtcTransmuter.Abstractions.Triggers;
using BtcTransmuter.Extension.NBXplorer.Models;
using BtcTransmuter.Extension.NBXplorer.Services;
using BtcTransmuter.Extension.NBXplorer.Triggers.NBXplorerNewBlock;
using BtcTransmuter.Extension.NBXplorer.Triggers.NBXplorerNewTransaction;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NBXplorer;
using NBXplorer.Models;

namespace BtcTransmuter.Extension.NBXplorer.HostedServices
{
    public class NBXplorerHostedService : IHostedService
    {
        private readonly NBXplorerOptions _options;
        private readonly NBXplorerClientProvider _nbXplorerClientProvider;
        private readonly NBXplorerSummaryProvider _nbXplorerSummaryProvider;
        private readonly ILogger<NBXplorerHostedService> _logger;
        private readonly ITriggerDispatcher _triggerDispatcher;

        public NBXplorerHostedService(NBXplorerOptions options,
            NBXplorerClientProvider nbXplorerClientProvider,
            NBXplorerSummaryProvider nbXplorerSummaryProvider, ILogger<NBXplorerHostedService> logger,
            ITriggerDispatcher triggerDispatcher)
        {
            _options = options;
            _nbXplorerClientProvider = nbXplorerClientProvider;
            _nbXplorerSummaryProvider = nbXplorerSummaryProvider;
            _logger = logger;
            _triggerDispatcher = triggerDispatcher;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            if (_options.Cryptos == null || !_options.Cryptos.Any() || _options.Uri != null)
            {
                return Task.CompletedTask;
            }

            foreach (var cryptoCode in _options.Cryptos)
            {
                var client = _nbXplorerClientProvider.GetClient(cryptoCode);

                _ = UpdateSummaryContinuously(client, cancellationToken);
            }

            return Task.CompletedTask;
        }

        private async Task UpdateSummaryContinuously(ExplorerClient explorerClient, CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                await _nbXplorerSummaryProvider.UpdateClientState(explorerClient, cancellationToken);
                var summary = _nbXplorerSummaryProvider.GetSummary(explorerClient.CryptoCode);
                await Task.Delay(TimeSpan.FromSeconds(summary.State == NBXplorerState.Ready ? 30 : 5),
                    cancellationToken);
            }
        }


        private async Task MonitorClientForTriggers(ExplorerClient explorerClient, CancellationToken cancellationToken)
        {
            WebsocketNotificationSession notificationSession = null;
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var summary = _nbXplorerSummaryProvider.GetSummary(explorerClient.CryptoCode);
                    if (summary.State != NBXplorerState.Ready)
                    {
                        await Task.Delay(TimeSpan.FromSeconds(10), cancellationToken);
                        continue;
                    }

                    notificationSession =
                        await explorerClient.CreateWebsocketNotificationSessionAsync(cancellationToken);

                    await notificationSession.ListenNewBlockAsync(cancellationToken);
                    await notificationSession.ListenAllTrackedSourceAsync(false, cancellationToken);

                    while (!cancellationToken.IsCancellationRequested)
                    {
                        var evt = await notificationSession.NextEventAsync(cancellationToken);
                        switch (evt)
                        {
                            case NewBlockEvent newBlockEvent:
                                await _triggerDispatcher.DispatchTrigger(new NBXplorerNewBlockTrigger()
                                {
                                    Data = new NBXplorerNewBlockTriggerData()
                                    {
                                        CryptoCode = evt.CryptoCode,
                                        Event = newBlockEvent
                                    }
                                });

                                break;
                            case NewTransactionEvent newTransactionEvent:
                            {
                                await _triggerDispatcher.DispatchTrigger(new NBXplorerNewTransactionTrigger()
                                {
                                    Data = new NBXplorerNewTransactionTriggerData()
                                    {
                                        CryptoCode = evt.CryptoCode,
                                        Event = newTransactionEvent
                                    }
                                });
                                break;
                            }
                            case UnknownEvent unknownEvent:
                                _logger.LogWarning(
                                    $"Received unknown message from NBXplorer ({unknownEvent.CryptoCode}), ID: {unknownEvent.EventId}");
                                break;
                        }
                    }
                }
                catch when (cancellationToken.IsCancellationRequested)
                {
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex,
                        $"Error while connecting to WebSocket of NBXplorer ({explorerClient.CryptoCode})");
                }
                finally
                {
                    if (notificationSession != null)
                    {
                        await notificationSession.DisposeAsync(cancellationToken);
                        notificationSession = null;
                    }
                }
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}