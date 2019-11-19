using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BtcTransmuter.Abstractions.ExternalServices;
using BtcTransmuter.Abstractions.Recipes;
using BtcTransmuter.Abstractions.Triggers;
using BtcTransmuter.Data.Models;
using BtcTransmuter.Extension.Lightning.ExternalServices.NBXplorerWallet;
using BtcTransmuter.Extension.NBXplorer.Models;
using BtcTransmuter.Extension.NBXplorer.Services;
using BtcTransmuter.Extension.NBXplorer.Triggers.NBXplorerBalance;
using BtcTransmuter.Extension.NBXplorer.Triggers.NBXplorerNewBlock;
using BtcTransmuter.Extension.NBXplorer.Triggers.NBXplorerNewTransaction;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NBitcoin;
using NBXplorer;
using NBXplorer.Models;
using Newtonsoft.Json.Linq;

namespace BtcTransmuter.Extension.NBXplorer.HostedServices
{
    public class NBXplorerHostedService : IHostedService
    {
        private readonly NBXplorerOptions _options;
        private readonly NBXplorerClientProvider _nbXplorerClientProvider;
        private readonly DerivationSchemeParser _derivationSchemeParser;
        private readonly NBXplorerPublicWalletProvider _nbXplorerPublicWalletProvider;
        private readonly IRecipeManager _recipeManager;
        private readonly IExternalServiceManager _externalServiceManager;
        private readonly NBXplorerSummaryProvider _nbXplorerSummaryProvider;
        private readonly ILogger<NBXplorerHostedService> _logger;
        private readonly ITriggerDispatcher _triggerDispatcher;

        public NBXplorerHostedService(NBXplorerOptions options,
            NBXplorerClientProvider nbXplorerClientProvider,
            DerivationSchemeParser derivationSchemeParser,
            NBXplorerPublicWalletProvider nbXplorerPublicWalletProvider,
            IRecipeManager recipeManager,
            IExternalServiceManager externalServiceManager,
            NBXplorerSummaryProvider nbXplorerSummaryProvider, ILogger<NBXplorerHostedService> logger,
            ITriggerDispatcher triggerDispatcher)
        {
            _options = options;
            _nbXplorerClientProvider = nbXplorerClientProvider;
            _derivationSchemeParser = derivationSchemeParser;
            _nbXplorerPublicWalletProvider = nbXplorerPublicWalletProvider;
            _recipeManager = recipeManager;
            _externalServiceManager = externalServiceManager;
            _nbXplorerSummaryProvider = nbXplorerSummaryProvider;
            _logger = logger;
            _triggerDispatcher = triggerDispatcher;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            if (_options.Cryptos == null || !_options.Cryptos.Any() || _options.Uri == null)
            {
                _logger.LogWarning($"NBXplorer not configured but plugin is loaded: {JObject.FromObject(_options)}");
                return Task.CompletedTask;
            }

            foreach (var cryptoCode in _options.Cryptos)
            {
                _logger.LogWarning($"Starting up listening to {cryptoCode}");
                var client = _nbXplorerClientProvider.GetClient(cryptoCode);
                _ = MonitorClientForTriggers(client, cancellationToken);
                _ = UpdateSummaryContinuously(client, cancellationToken);
            }

            _ = UpdateBalances_Continuously(cancellationToken);
            return Task.CompletedTask;
        }

        private async Task UpdateSummaryContinuously(ExplorerClient explorerClient, CancellationToken cancellationToken)
        {
            await explorerClient.WaitServerStartedAsync(cancellationToken);
            while (!cancellationToken.IsCancellationRequested)
            {
                await _nbXplorerSummaryProvider.UpdateClientState(explorerClient, cancellationToken);
                var summary = _nbXplorerSummaryProvider.GetSummary(explorerClient.CryptoCode);
                await Task.Delay(TimeSpan.FromSeconds(summary.State == NBXplorerState.Ready ? 30 : 5),
                    cancellationToken);
            }
        }

        private async Task UpdateBalances_Continuously(
            CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var walletExternalServices = await
                    _externalServiceManager.GetExternalServicesData(new ExternalServicesDataQuery()
                    {
                        Type = new string[]
                        {
                            NBXplorerWalletService.NBXplorerWalletServiceType
                        }
                    });

                var groupedWalletEService = walletExternalServices
                    .Select(data => new NBXplorerWalletService(data, _nbXplorerPublicWalletProvider,
                        _derivationSchemeParser, _nbXplorerClientProvider))
                    .Select(data => (data, data.GetData()))
                    .GroupBy(
                        tuple => $"{tuple.Item2.CryptoCode}_{tuple.Item2.Address}_{tuple.Item2.DerivationStrategy}")
                    .ToList();


                foreach (var valueTuples in groupedWalletEService)
                {
                    var first = valueTuples.First();

                    var wallet = await first.Item1.ConstructClient();

                    var explorerClient = _nbXplorerClientProvider.GetClient(first.Item2.CryptoCode);


                    var balance = await wallet.GetBalance();

                    _ =  _triggerDispatcher.DispatchTrigger(new NBXplorerBalanceTrigger(explorerClient)
                    {
                        Data = new NBXplorerBalanceTriggerData()
                        {
                            Balance = balance,
                            CryptoCode = explorerClient.CryptoCode,
                            TrackedSource = wallet.TrackedSource
                        }
                    });
                }

                await Task.Delay(TimeSpan.FromMinutes(1),
                    cancellationToken);
            }
        }


        private async Task MonitorClientForTriggers(ExplorerClient explorerClient, CancellationToken cancellationToken)
        {
            await explorerClient.WaitServerStartedAsync(cancellationToken);
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
                        var factory = explorerClient.Network.DerivationStrategyFactory;

                        switch (evt)
                        {
                            case NewBlockEvent newBlockEvent:
                                _ =  _triggerDispatcher.DispatchTrigger(new NBXplorerNewBlockTrigger()
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
                                    TriggerId = NBXplorerNewTransactionTrigger.Id
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
                                            var walletService = new NBXplorerWalletService(
                                                recipe.RecipeTrigger.ExternalService, _nbXplorerPublicWalletProvider,
                                                _derivationSchemeParser,
                                                _nbXplorerClientProvider);
                                            
                                            
                                            
                                            _ =  _triggerDispatcher.DispatchTrigger(
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
                                                            TrackedSource = await walletService.ConstructTrackedSource()
                                                        }
                                                    }
                                                });
                                        }
                                    }
                                }

                                break;
                            case NewTransactionEvent newTransactionEvent:
                            {
                                _ =  _triggerDispatcher.DispatchTrigger(
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