using System.Collections.Generic;
using BtcTransmuter.Extension.NBXplorer.Models;
using NBitcoin;
using NBXplorer;

namespace BtcTransmuter.Extension.NBXplorer.Services
{
    public class NBXplorerClientProvider
    {
        private readonly NBXplorerOptions _options;
        private Dictionary<string, ExplorerClient> _clients = new Dictionary<string, ExplorerClient>();
        private NBXplorerNetworkProvider _nbXplorerNetworkProvider;

        public NBXplorerClientProvider(NBXplorerOptions options)
        {
            _options = options;
            
            _nbXplorerNetworkProvider = new NBXplorerNetworkProvider(_options.NetworkType);
        }

        public ExplorerClient GetClient(string cryptoCode)
        {
            if (_clients.ContainsKey(cryptoCode))
            {
                return _clients[cryptoCode];
            }
            var client = new ExplorerClient(_nbXplorerNetworkProvider.GetFromCryptoCode(cryptoCode), _options.Uri);

            if (string.IsNullOrEmpty(_options.CookieFile))
            {
                client.SetNoAuth();
            }
            else
            {
                client.SetCookieAuth(_options.CookieFile);
            }
            
            _clients.AddOrReplace(cryptoCode, client);
            return client;
        }
        
        
    }
}