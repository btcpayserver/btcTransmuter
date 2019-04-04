using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BtcTransmuter.Abstractions.Actions;
using BtcTransmuter.Data.Entities;
using BtcTransmuter.Extension.DynamicServices;
using BtcTransmuter.Extension.Lightning.ExternalServices.NBXplorerWallet;
using BtcTransmuter.Extension.NBXplorer.Actions.GenerateNextAddress;
using BtcTransmuter.Extension.NBXplorer.Services;
using NBitcoin;
using NBitcoin.BIP174;
using NBXplorer.Models;

namespace BtcTransmuter.Extension.NBXplorer.Actions.NBXplorerCreatePSBT
{
    public class NBXplorerCreatePSBTDataActionHandler : BaseActionHandler<NBXplorerCreatePSBTData, PSBT>
    {
        private readonly NBXplorerPublicWalletProvider _nbXplorerPublicWalletProvider;
        private readonly NBXplorerClientProvider _nbXplorerClientProvider;
        private readonly DerivationStrategyFactoryProvider _derivationStrategyFactoryProvider;
        private readonly DerivationSchemeParser _derivationSchemeParser;
        public override string ActionId => "NBXplorerCreatePSBT";
        public override string Name => "Create PSBT";

        public override string Description =>
            "Create a partially signed built transaction";

        public override string ViewPartial => "ViewNBXplorerCreatePSBTAction";

        public override string ControllerName => "NBXplorerCreatePSBT";

        public NBXplorerCreatePSBTDataActionHandler()
        {
        }

        public NBXplorerCreatePSBTDataActionHandler(
            NBXplorerPublicWalletProvider nbXplorerPublicWalletProvider,
            NBXplorerClientProvider nbXplorerClientProvider,
            DerivationStrategyFactoryProvider derivationStrategyFactoryProvider,
            DerivationSchemeParser derivationSchemeParser)
        {
            _nbXplorerPublicWalletProvider = nbXplorerPublicWalletProvider;
            _nbXplorerClientProvider = nbXplorerClientProvider;
            _derivationStrategyFactoryProvider = derivationStrategyFactoryProvider;
            _derivationSchemeParser = derivationSchemeParser;
        }

        protected override async Task<TypedActionHandlerResult<PSBT>> Execute(
            Dictionary<string, object> data, RecipeAction recipeAction,
            NBXplorerCreatePSBTData actionData)
        {
            
            var externalService = await recipeAction.GetExternalService();
            var walletService = new NBXplorerWalletService(externalService, _nbXplorerPublicWalletProvider,
                _derivationSchemeParser, _derivationStrategyFactoryProvider, _nbXplorerClientProvider);

            var wallet = await walletService.ConstructClient();
            var walletData = walletService.GetData();
            var explorerClient = _nbXplorerClientProvider.GetClient(walletData.CryptoCode);
            var tx = await wallet.BuildTransaction(actionData.Outputs.Select(output =>
                    (Money.Parse(InterpolateString(output.Amount, data)),
                        (IDestination) BitcoinAddress.Create(InterpolateString(output.DestinationAddress, data),
                            explorerClient.Network.NBitcoinNetwork), output.SubtractFeesFromOutput)),
                walletData.PrivateKeys);

            var psbt = PSBT.FromTransaction(tx);
            
            return new NBXplorerActionHandlerResult<PSBT>(
                _nbXplorerClientProvider.GetClient(walletData.CryptoCode))
            {
                Executed = true,
                TypedData = psbt,
                Result = $"PSBT created {psbt}"
            };
        }
    }
}