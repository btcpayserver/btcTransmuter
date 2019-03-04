using System.Collections.Generic;
using NBXplorer;
using NBXplorer.DerivationStrategy;

namespace BtcTransmuter.Extension.NBXplorer.Services
{
    public class DerivationStrategyFactoryProvider
    {
        private readonly NBXplorerNetworkProvider _nbXplorerNetworkProvider;
        private Dictionary<string, DerivationStrategyFactory> _derivationStrategyFactories = new Dictionary<string, DerivationStrategyFactory>();
        public DerivationStrategyFactoryProvider(NBXplorerNetworkProvider nbXplorerNetworkProvider)
        {
            _nbXplorerNetworkProvider = nbXplorerNetworkProvider;
        }

        public DerivationStrategyFactory GetDerivationStrategyFactory(string cryptoCode)
        {
            if (_derivationStrategyFactories.ContainsKey(cryptoCode))
            {
                return _derivationStrategyFactories[cryptoCode];
            }
            _derivationStrategyFactories.Add(cryptoCode, new DerivationStrategyFactory(_nbXplorerNetworkProvider.GetFromCryptoCode(cryptoCode).NBitcoinNetwork));
            return GetDerivationStrategyFactory(cryptoCode);
        }
    }
}