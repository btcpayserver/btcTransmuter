using System.Collections.Generic;
using BtcTransmuter.Extension.NBXplorer.Models;
using Microsoft.Extensions.Options;
using NBitcoin;
using NBXplorer;

namespace BtcTransmuter.Extension.NBXplorer.HostedServices
{
    public class NBXplorerClientProvider
    {
        private readonly IOptions<NBXplorerOptions> _options;
        private Dictionary<string, ExplorerClient> _clients = new Dictionary<string, ExplorerClient>();
        private NBXplorerNetworkProvider _nbXplorerNetworkProvider;

        public NBXplorerClientProvider(IOptions<NBXplorerOptions> options)
        {
            _options = options;
            
            _nbXplorerNetworkProvider = new NBXplorerNetworkProvider(_options.Value.NetworkType);
        }

        public ExplorerClient GetClient(string cryptoCode)
        {
            if (_clients.ContainsKey(cryptoCode))
            {
                return _clients[cryptoCode];
            }
            var client = new ExplorerClient(_nbXplorerNetworkProvider.GetFromCryptoCode(cryptoCode));

            if (string.IsNullOrEmpty(_options.Value.CookieFile))
            {
                client.SetNoAuth();
            }
            else
            {
                client.SetCookieAuth(_options.Value.CookieFile);
            }
            
            _clients.AddOrReplace(cryptoCode, client);
            return client;
        }
        
        
    }
}