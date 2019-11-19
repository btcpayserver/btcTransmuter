using System.Collections.Generic;
using System.Threading.Tasks;
using BtcTransmuter.Abstractions.Actions;
using BtcTransmuter.Data.Entities;
using BtcTransmuter.Extension.DynamicServices;
using BtcTransmuter.Extension.Lightning.ExternalServices.NBXplorerWallet;
using BtcTransmuter.Extension.NBXplorer.Actions.GenerateNextAddress;
using BtcTransmuter.Extension.NBXplorer.Services;
using NBitcoin;

namespace BtcTransmuter.Extension.NBXplorer.Actions.NBXplorerGetBalance
{
    public class NBXplorerGetBalanceDataActionHandler : BaseActionHandler<NBXplorerGetBalanceData, Money>
    {
        private readonly NBXplorerClientProvider _nbXplorerClientProvider;
        private readonly NBXplorerPublicWalletProvider _nbXplorerPublicWalletProvider;
        private readonly DerivationSchemeParser _derivationSchemeParser;
        public override string ActionId => "NBXplorerGetBalance";
        public override string Name => "Get balance of wallet";

        public override string Description =>
            "Gte the balance of an NBXplorer Wallet external service";

        public override string ViewPartial => "ViewNBXplorerGetBalanceAction";

        public override string ControllerName => "NBXplorerGetBalance";

        public NBXplorerGetBalanceDataActionHandler()
        {
        }

        public NBXplorerGetBalanceDataActionHandler(
            NBXplorerClientProvider nbXplorerClientProvider,
            NBXplorerPublicWalletProvider nbXplorerPublicWalletProvider,
            DerivationSchemeParser derivationSchemeParser)
        {
            _nbXplorerClientProvider = nbXplorerClientProvider;
            _nbXplorerPublicWalletProvider = nbXplorerPublicWalletProvider;
            _derivationSchemeParser = derivationSchemeParser;
        }

        protected override async Task<TypedActionHandlerResult<Money>> Execute(Dictionary<string, object> data, RecipeAction recipeAction,
            NBXplorerGetBalanceData actionData)
        {

            var externalService = await recipeAction.GetExternalService();
            var walletService = new NBXplorerWalletService(externalService, _nbXplorerPublicWalletProvider,
                _derivationSchemeParser, _nbXplorerClientProvider);
            var walletData = walletService.GetData();
            var wallet = await walletService.ConstructClient();
            var result = await
                wallet.GetBalance();
            
            return new NBXplorerActionHandlerResult<Money>(_nbXplorerClientProvider.GetClient(walletData.CryptoCode))
            {
                Executed = true,
                TypedData = result,
                Result = $"Balance: {result}"
            };
        }
    }
}