using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BtcTransmuter.Abstractions.Recipes;
using BtcTransmuter.Abstractions.Triggers;
using BtcTransmuter.Data.Models;
using BtcTransmuter.Extension.NBXplorer.Models;
using BtcTransmuter.Extension.NBXplorer.Services;
using BtcTransmuter.Extension.NBXplorer.Triggers.NBXplorerNewBlock;
using BtcTransmuter.Extension.NBXplorer.Triggers.NBXplorerNewTransaction;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NBitcoin;
using NBitcoin.Logging;
using NBXplorer;
using NBXplorer.DerivationStrategy;
using NBXplorer.Models;

namespace BtcTransmuter.Extension.NBXplorer.HostedServices
{
    public class NBXplorerHostedService : IHostedService
    {
        private readonly NBXplorerOptions _options;
        private readonly NBXplorerClientProvider _nbXplorerClientProvider;
        private readonly DerivationSchemeParser _derivationSchemeParser;
        private readonly DerivationStrategyFactoryProvider _derivationStrategyFactoryProvider;
        private readonly IRecipeManager _recipeManager;
        private readonly NBXplorerSummaryProvider _nbXplorerSummaryProvider;
        private readonly ILogger<NBXplorerHostedService> _logger;
        private readonly ITriggerDispatcher _triggerDispatcher;

        public NBXplorerHostedService(NBXplorerOptions options,
            NBXplorerClientProvider nbXplorerClientProvider,
            DerivationSchemeParser derivationSchemeParser,
            DerivationStrategyFactoryProvider derivationStrategyFactoryProvider,
            IRecipeManager recipeManager,
            NBXplorerSummaryProvider nbXplorerSummaryProvider, ILogger<NBXplorerHostedService> logger,
            ITriggerDispatcher triggerDispatcher)
        {
            _options = options;
            _nbXplorerClientProvider = nbXplorerClientProvider;
            _derivationSchemeParser = derivationSchemeParser;
            _derivationStrategyFactoryProvider = derivationStrategyFactoryProvider;
            _recipeManager = recipeManager;
            _nbXplorerSummaryProvider = nbXplorerSummaryProvider;
            _logger = logger;
            _triggerDispatcher = triggerDispatcher;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            if (_options.Cryptos == null || !_options.Cryptos.Any() || _options.Uri == null)
            {
                return Task.CompletedTask;
            }

            foreach (var cryptoCode in _options.Cryptos)
            {
                var client = _nbXplorerClientProvider.GetClient(cryptoCode);
                _ = MonitorClientForTriggers(client, cancellationToken);
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
                    if (summary?.State != NBXplorerState.Ready)
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

                        var factory =
                            _derivationStrategyFactoryProvider.GetDerivationStrategyFactory(
                                evt.CryptoCode);

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
                                //we need to trigger transaction events for previous unconfirmed txs  so that they are checked again and trigger respective actions
                                var recipes = await _recipeManager.GetRecipes(new RecipesQuery()
                                {
                                    Enabled = true,
                                    RecipeTriggerId = NBXplorerNewTransactionTrigger.Id
                                });
                                foreach (var recipe in recipes)
                                {
                                    var triggerParameters =
                                        recipe.RecipeTrigger.Get<NBXplorerNewTransactionTriggerParameters>();
                                    if (triggerParameters.Transactions == null || !triggerParameters.Transactions.Any())
                                    {
                                        continue;
                                    }

                                    var tasks = triggerParameters.Transactions.Select(result =>
                                        (result,
                                            explorerClient.GetTransactionAsync(result.TransactionHash,
                                                cancellationToken)));
                                    await Task.WhenAll(tasks.Select(tuple => tuple.Item2));


                                    foreach (var tx in tasks)
                                    {
                                        if (tx.Item1.Confirmations != tx.Item2.Result.Confirmations)
                                        {
                                            await _triggerDispatcher.DispatchTrigger(
                                                new NBXplorerNewTransactionTrigger(explorerClient)
                                                {
                                                    Data = new NBXplorerNewTransactionTriggerData()
                                                    {
                                                        CryptoCode = evt.CryptoCode,
                                                        Event = new NewTransactionEvent()
                                                        {
                                                            CryptoCode = evt.CryptoCode,
                                                            BlockId = newBlockEvent.Hash,
                                                            TransactionData = tx.Item2.Result,
                                                            TrackedSource =
                                                                string.IsNullOrEmpty(triggerParameters
                                                                    .DerivationStrategy)
                                                                    ? (TrackedSource) TrackedSource.Create(
                                                                        BitcoinAddress.Create(triggerParameters.Address,
                                                                            explorerClient.Network.NBitcoinNetwork))
                                                                    : (TrackedSource) TrackedSource.Create(
                                                                        _derivationSchemeParser.Parse(factory,
                                                                            triggerParameters.DerivationStrategy)),
                                                            DerivationStrategy =
                                                                string.IsNullOrEmpty(triggerParameters
                                                                    .DerivationStrategy)
                                                                    ? null
                                                                    : _derivationSchemeParser.Parse(factory,
                                                                        triggerParameters.DerivationStrategy),
                                                        }
                                                    }
                                                });
                                        }
                                    }
                                }

                                break;
                            case NewTransactionEvent newTransactionEvent:
                            {
                                await _triggerDispatcher.DispatchTrigger(
                                    new NBXplorerNewTransactionTrigger(explorerClient)
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