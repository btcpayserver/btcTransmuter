using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BtcTransmuter.Abstractions.Triggers;
using BtcTransmuter.Extension.NBXplorer.Models;
using BtcTransmuter.Extension.NBXplorer.Triggers.NBXplorerNewBlock;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NBXplorer;
using NBXplorer.Models;

namespace BtcTransmuter.Extension.NBXplorer.HostedServices
{
    public class NBXplorerHostedService : IHostedService
    {
        private readonly IOptions<NBXplorerOptions> _options;
        private readonly NBXplorerSummaryProvider _nbXplorerSummaryProvider;
        private readonly ILogger<NBXplorerHostedService> _logger;
        private readonly ITriggerDispatcher _triggerDispatcher;
        private NBXplorerNetworkProvider _nbXplorerNetworkProvider;
        private Dictionary<string, ExplorerClient> _clients;

        public NBXplorerHostedService(IOptions<NBXplorerOptions> options,
            NBXplorerSummaryProvider nbXplorerSummaryProvider, ILogger<NBXplorerHostedService> logger,
            ITriggerDispatcher triggerDispatcher)
        {
            _options = options;
            _nbXplorerSummaryProvider = nbXplorerSummaryProvider;
            _logger = logger;
            _triggerDispatcher = triggerDispatcher;
            _nbXplorerNetworkProvider = new NBXplorerNetworkProvider(_options.Value.NetworkType);
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            if (_options.Value.Cryptos == null || !_options.Value.Cryptos.Any() || _options.Value.Uri != null)
            {
                return Task.CompletedTask;
            }

            _clients = _options.Value.Cryptos.Select(s =>
            {
                var client = new ExplorerClient(_nbXplorerNetworkProvider.GetFromCryptoCode(s));

                if (string.IsNullOrEmpty(_options.Value.CookieFile))
                {
                    client.SetNoAuth();
                }
                else
                {
                    client.SetCookieAuth(_options.Value.CookieFile);
                }

                _ = UpdateSummaryContinuously(client, cancellationToken);
                return client;
            }).ToDictionary(client => client.CryptoCode, client => client);
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
                    await notificationSession.ListenAllDerivationSchemesAsync(false, cancellationToken);

                    while (!cancellationToken.IsCancellationRequested)
                    {
                        var evt = await notificationSession.NextEventAsync(cancellationToken);
                        switch (evt)
                        {
                            case NewBlockEvent newBlockEvent:
                                var trigger = new NBXplorerNewBlockTrigger()
                                {
                                    Data = new NBXplorerNewBlockTriggerData()
                                    {
                                        CryptoCode = evt.CryptoCode,
                                        Event = newBlockEvent
                                    }
                                };
                                await _triggerDispatcher.DispatchTrigger(trigger);

                                break;
                            case NewTransactionEvent newTransactionEvent:
                                //TODO: dispatch a bunch of different triggers
                                break;
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