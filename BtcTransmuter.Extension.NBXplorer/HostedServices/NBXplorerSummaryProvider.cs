using System;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using BtcTransmuter.Extension.NBXplorer.Models;
using Microsoft.Extensions.Options;
using NBitcoin;
using NBXplorer;
using NBXplorer.Models;

namespace BtcTransmuter.Extension.NBXplorer.HostedServices
{
    public class NBXplorerSummaryProvider
    {
        private readonly IOptions<NBXplorerOptions> _options;

        private readonly ConcurrentDictionary<string, NBXplorerSummary>
            _summaries = new ConcurrentDictionary<string, NBXplorerSummary>();

        public NBXplorerSummaryProvider(IOptions<NBXplorerOptions> options)
        {
            _options = options;
        }

        public ImmutableDictionary<string, NBXplorerSummary> GetSummaries()
        {
            return _summaries.ToImmutableDictionary(pair => pair.Key, pair => pair.Value);
        }

        public NBXplorerSummary GetSummary(string cryptoCode)
        {
            return _summaries.TryGet(cryptoCode);
        }

        public async Task UpdateClientState(ExplorerClient client, CancellationToken cancellation)
        {
            var oldState = _summaries.TryGet(client.CryptoCode);
            var state = (NBXplorerState?) null;
            string error = null;
            StatusResult status = null;
            try
            {
                switch (oldState?.State)
                {
                    case null:
                        break;
                    case NBXplorerState.NotConnected:
                        status = await client.GetStatusAsync(cancellation);
                        if (status != null)
                        {
                            state = status.IsFullySynched ? NBXplorerState.Ready : NBXplorerState.Synching;
                        }

                        break;
                    case NBXplorerState.Synching:
                        status = await client.GetStatusAsync(cancellation);
                        if (status == null)
                        {
                            state = NBXplorerState.NotConnected;
                        }
                        else if (status.IsFullySynched)
                        {
                            state = NBXplorerState.Ready;
                        }

                        break;
                    case NBXplorerState.Ready:
                        status = await client.GetStatusAsync(cancellation);
                        if (status == null)
                        {
                            state = NBXplorerState.NotConnected;
                        }
                        else if (!status.IsFullySynched)
                        {
                            state = NBXplorerState.Synching;
                        }

                        break;
                }
            }
            catch (Exception ex) when (!cancellation.IsCancellationRequested)
            {
                error = ex.Message;
            }

            if (status != null && error == null)
            {
                if (status.NetworkType != _options.Value.NetworkType)
                    error =
                        $"{client.CryptoCode}: NBXplorer is on a different ChainType (actual: {status.NetworkType}, expected: {_options.Value.NetworkType})";
            }

            if (error != null)
            {
                state = NBXplorerState.NotConnected;
                status = null;
            }

            var result = await client.GetStatusAsync(cancellation);
            _summaries.AddOrReplace(client.CryptoCode, new NBXplorerSummary()
            {
                Status = result,
                State = state.GetValueOrDefault(NBXplorerState.NotConnected),
                Error = error
            });
        }
    }
}