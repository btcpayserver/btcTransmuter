using System.Collections.Generic;
using BtcTransmuter.Extension.NBXplorer.Models;
using Microsoft.Extensions.Logging;
using NBitcoin;
using NBXplorer;

namespace BtcTransmuter.Extension.NBXplorer.Services
{
    public class NBXplorerClientProvider
    {
        private readonly NBXplorerOptions _options;
        private readonly ILogger<NBXplorerClientProvider> _logger;
        private Dictionary<string, ExplorerClient> _clients = new Dictionary<string, ExplorerClient>();
        private NBXplorerNetworkProvider _nbXplorerNetworkProvider;

        public NBXplorerClientProvider(NBXplorerOptions options, ILogger<NBXplorerClientProvider> logger)
        {
            _options = options;
            _logger = logger;

            _nbXplorerNetworkProvider = new NBXplorerNetworkProvider(_options.NetworkType);
        }

        public ExplorerClient GetClient(string cryptoCode)
        {
            if (_clients.ContainsKey(cryptoCode))
            {
                return _clients[cryptoCode];
            }

            _logger.LogWarning($"Creating NBXplorer Client {cryptoCode}");
            var network = _nbXplorerNetworkProvider.GetFromCryptoCode(cryptoCode);
            if (network == null)
            {
                _logger.LogWarning($"Skipping {cryptoCode}");
                return null;
            }
            var client = new ExplorerClient(network, _options.Uri);

            if (string.IsNullOrEmpty(_options.CookieFile) && !_options.UseDefaultCookie)
            {
                _logger.LogWarning($"Connecting to NBXplorer @{_options.Uri} with no auth");
                client.SetNoAuth();
            }
            else if (string.IsNullOrEmpty(_options.CookieFile)  && _options.UseDefaultCookie)
            {
                _logger.LogWarning(
                    $"Connecting to NBXplorer @{_options.Uri} with default cookie {client.Network.DefaultSettings.DefaultCookieFile}");
                client.SetCookieAuth(client.Network.DefaultSettings.DefaultCookieFile);
            }
            else
            {
                _logger.LogWarning(
                    $"Connecting to NBXplorer @{_options.Uri} with auth cookie {_options.CookieFile}");

                client.SetCookieAuth(_options.CookieFile);
            }

            _clients.AddOrReplace(cryptoCode, client);
            return client;
        }
    }
}