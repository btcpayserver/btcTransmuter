using System;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using BtcTransmuter.Extension.NBXplorer.Models;
using Microsoft.Extensions.Logging;
using NBitcoin;
using NBXplorer;
using NBXplorer.Models;
using Newtonsoft.Json.Linq;

namespace BtcTransmuter.Extension.NBXplorer.Services
{
    public class NBXplorerSummaryProvider
    {
        private readonly NBXplorerOptions _options;
        private readonly ILogger<NBXplorerSummaryProvider> _logger;

        private readonly ConcurrentDictionary<string, NBXplorerSummary>
            _summaries = new ConcurrentDictionary<string, NBXplorerSummary>();

        public NBXplorerSummaryProvider(NBXplorerOptions options, ILogger<NBXplorerSummaryProvider> logger)
        {
            _options = options;
            _logger = logger;
        }

        public ImmutableDictionary<string, NBXplorerSummary> GetSummaries()
        {
            return _summaries.ToImmutableDictionary(pair => pair.Key.ToUpperInvariant(), pair => pair.Value);
        }

        public NBXplorerSummary GetSummary(string cryptoCode)
        {
            return _summaries.TryGet(cryptoCode.ToUpperInvariant());
        }

        public async Task UpdateClientState(ExplorerClient client, CancellationToken cancellation)
        {
            _logger.LogInformation($"Updating summary for {client.CryptoCode}");
            var state = (NBXplorerState?) null;
            string error = null;
            StatusResult status = null;
            try
            {
                status = await client.GetStatusAsync(cancellation);
                if (status == null)
                {
                    state = NBXplorerState.NotConnected;
                }
                else if (status.IsFullySynched)
                {
                    state = NBXplorerState.Ready;
                }
                else if (!status.IsFullySynched)
                {
                    state = NBXplorerState.Synching;
                }
            }
            catch (Exception ex) when (!cancellation.IsCancellationRequested)
            {
                _logger.LogWarning($"Could not update summary for {client.CryptoCode} because {ex.Message}");
                error = ex.Message;
            }

            if (status != null && error == null && status.NetworkType != _options.NetworkType)
            {
                error =
                    $"{client.CryptoCode}: NBXplorer is on a different ChainType (actual: {status.NetworkType}, expected: {_options.NetworkType})";
            }

            if (error != null)
            {
                state = NBXplorerState.NotConnected;
            }

            var summary = new NBXplorerSummary()
            {
                Status = status,
                State = state.GetValueOrDefault(NBXplorerState.NotConnected),
                Error = error
            };
            _logger.LogInformation($"summary updated {client.CryptoCode}");
            _summaries.AddOrReplace(client.CryptoCode.ToUpperInvariant(), summary);
        }
    }
}