using System.Threading.Tasks;
using BtcTransmuter.Abstractions.ExternalServices;
using BtcTransmuter.Data.Entities;
using BtcTransmuter.Extension.NBXplorer.Services;
using NBitcoin;
using NBXplorer.Models;

namespace BtcTransmuter.Extension.Lightning.ExternalServices.NBXplorerWallet
{
    public class NBXplorerWalletService : BaseExternalService<NBXplorerWalletExternalServiceData>
    {
        private readonly NBXplorerPublicWalletProvider _nbXplorerPublicWalletProvider;
        private readonly DerivationSchemeParser _derivationSchemeParser;
        private readonly NBXplorerClientProvider _nbXplorerClientProvider;
        public const string NBXplorerWalletServiceType = "NBXplorerWalletExternalService";
        public override string ExternalServiceType => NBXplorerWalletServiceType;

        public override string Name => "NBXplorerWallet External Service";
        public override string Description => "Track Addresses, HD wallets and sign txs";
        public override string ViewPartial => "ViewNBXplorerWalletExternalService";
        public override string ControllerName => "NBXplorerWallet";

        public NBXplorerWalletService() : base()
        {
        }

        public NBXplorerWalletService(ExternalServiceData data,
            NBXplorerPublicWalletProvider nbXplorerPublicWalletProvider,
            DerivationSchemeParser derivationSchemeParser,
            NBXplorerClientProvider nbXplorerClientProvider) : base(data)
        {
            _nbXplorerPublicWalletProvider = nbXplorerPublicWalletProvider;
            _derivationSchemeParser = derivationSchemeParser;
            _nbXplorerClientProvider = nbXplorerClientProvider;
        }

        public async Task<NBXplorerPublicWallet> ConstructClient()
        {
            var data = GetData();

            var explorerClient = _nbXplorerClientProvider.GetClient(data.CryptoCode);
            var factory = explorerClient.Network.DerivationStrategyFactory;
            if (string.IsNullOrEmpty(data
                .DerivationStrategy))
            {
                return await _nbXplorerPublicWalletProvider.Get(data.CryptoCode,
                    BitcoinAddress.Create(
                        data.Address,
                        explorerClient.Network.NBitcoinNetwork));
            }
            else
            {
                return await _nbXplorerPublicWalletProvider.Get(data.CryptoCode,
                    _derivationSchemeParser.Parse(factory,
                        data.DerivationStrategy));
            }
        }

        public Task<TrackedSource> ConstructTrackedSource()
        {
            var data = GetData();
            
            var explorerClient = _nbXplorerClientProvider.GetClient(data.CryptoCode);
            var factory = explorerClient.Network.DerivationStrategyFactory;
            if (string.IsNullOrEmpty(data
                .DerivationStrategy))
            {
                return Task.FromResult<TrackedSource>(TrackedSource.Create(BitcoinAddress.Create(
                    data.Address,
                    explorerClient.Network.NBitcoinNetwork)));
            }
            else
            {
                return Task.FromResult<TrackedSource>(TrackedSource.Create( _derivationSchemeParser.Parse(factory,
                    data.DerivationStrategy)));
            }
        }
    }
}