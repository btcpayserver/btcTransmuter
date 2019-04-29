using System.Collections.Generic;
using System.Threading.Tasks;
using BtcTransmuter.Abstractions.Actions;
using BtcTransmuter.Data.Entities;
using BtcTransmuter.Extension.DynamicServices;
using BtcTransmuter.Extension.Lightning.ExternalServices.NBXplorerWallet;
using BtcTransmuter.Extension.NBXplorer.Actions.GenerateNextAddress;
using BtcTransmuter.Extension.NBXplorer.Services;
using NBitcoin;
using NBitcoin.BIP174;

namespace BtcTransmuter.Extension.NBXplorer.Actions.NBXplorerSignPSBT
{
//    public class NBXplorerSignPSBTDataActionHandler : BaseActionHandler<NBXplorerSignPSBTData, PSBT>
//    {
//        private readonly NBXplorerClientProvider _nbXplorerClientProvider;
//        private readonly NBXplorerPublicWalletProvider _nbXplorerPublicWalletProvider;
//        private readonly DerivationStrategyFactoryProvider _derivationStrategyFactoryProvider;
//        private readonly DerivationSchemeParser _derivationSchemeParser;
//        public override string ActionId => "NBXplorerSignPSBT";
//        public override string Name => "Sign PSBT";
//
//        public override string Description =>
//            "Sign a PSBT with your own keys";
//
//        public override string ViewPartial => "ViewNBXplorerSignPSBTAction";
//
//        public override string ControllerName => "NBXplorerSignPSBT";
//
//        public NBXplorerSignPSBTDataActionHandler()
//        {
//        }
//
//        public NBXplorerSignPSBTDataActionHandler(
//            NBXplorerClientProvider nbXplorerClientProvider,
//            NBXplorerPublicWalletProvider nbXplorerPublicWalletProvider,
//            DerivationStrategyFactoryProvider derivationStrategyFactoryProvider,
//            DerivationSchemeParser derivationSchemeParser)
//        {
//            _nbXplorerClientProvider = nbXplorerClientProvider;
//            _nbXplorerPublicWalletProvider = nbXplorerPublicWalletProvider;
//            _derivationStrategyFactoryProvider = derivationStrategyFactoryProvider;
//            _derivationSchemeParser = derivationSchemeParser;
//        }
//
//        protected override async Task<TypedActionHandlerResult<PSBT>> Execute(Dictionary<string, object> data, RecipeAction recipeAction,
//            NBXplorerSignPSBTData actionData)
//        {
//
//            var externalService = await recipeAction.GetExternalService();
//            var walletService = new NBXplorerWalletService(externalService, _nbXplorerPublicWalletProvider,
//                _derivationSchemeParser, _derivationStrategyFactoryProvider, _nbXplorerClientProvider);
//            var walletData = walletService.GetData();
//            var wallet = await walletService.ConstructClient();
//
//            var psbt = PSBT.Parse(actionData.PSBT);
//            var tx = psbt.ExtractTX();
//            var tBuilder = await wallet.CreateTransactionBuilder();
//            tBuilder = tBuilder.ContinueToBuild(tx);
//            foreach (var privateKeyDetails in walletData.PrivateKeys)
//            {
//                await wallet.SignTransaction(tBuilder, privateKeyDetails);
//            }
//
//            psbt = PSBT.FromTransaction(
//                tBuilder.BuildTransaction(true));
//           
//            return new NBXplorerActionHandlerResult<PSBT>(_nbXplorerClientProvider.GetClient(walletData.CryptoCode))
//            {
//                Executed = true,
//                TypedData = psbt,
//                Result = $"signed psbt: {psbt}"
//            };
//        }
//    }
}