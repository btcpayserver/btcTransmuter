using System.Collections.Generic;
using System.Threading.Tasks;
using NBitcoin;
using NBXplorer.DerivationStrategy;
using NBXplorer.Models;

namespace BtcTransmuter.Extension.NBXplorer.Services
{
    public class NBXplorerPublicWalletProvider
    {
        private readonly NBXplorerClientProvider _clientProvider;
        private Dictionary<string, NBXplorerPublicWallet> _wallets = new Dictionary<string, NBXplorerPublicWallet>();
        public NBXplorerPublicWalletProvider(NBXplorerClientProvider clientProvider)
        {
            _clientProvider = clientProvider;
        }

        public async Task<NBXplorerPublicWallet> Get(string cryptoCode, BitcoinAddress address)
        {
            var key = $"{cryptoCode}:{address}";
            if (_wallets.ContainsKey(key))
            {
                return _wallets[key];
            }
            _wallets.Add(key,new NBXplorerPublicWallet(_clientProvider.GetClient(cryptoCode), TrackedSource.Create(address)));
            return await Get(cryptoCode, address);
        }
        
        public async Task<NBXplorerPublicWallet> Get(string cryptoCode, DerivationStrategyBase derivationStrategyBase)
        {
            var key = $"{cryptoCode}:{derivationStrategyBase}";
            if (_wallets.ContainsKey(key))
            {
                return _wallets[key];
            }
            _wallets.Add(key,new NBXplorerPublicWallet(_clientProvider.GetClient(cryptoCode), TrackedSource.Create(derivationStrategyBase)));
            return await Get(cryptoCode, derivationStrategyBase);
        }
    }
}