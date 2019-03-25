using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BtcTransmuter.Abstractions.Actions;
using BtcTransmuter.Data.Entities;
using BtcTransmuter.Extension.NBXplorer.Services;
using NBitcoin;
using NBXplorer.Models;

namespace BtcTransmuter.Extension.NBXplorer.Actions.SendTransaction
{
    public class SendTransactionDataActionHandler : BaseActionHandler<SendTransactionData, BroadcastResult>
    {
        private readonly NBXplorerPublicWalletProvider _nbXplorerPublicWalletProvider;
        private readonly NBXplorerClientProvider _nbXplorerClientProvider;
        private readonly DerivationStrategyFactoryProvider _derivationStrategyFactoryProvider;
        private readonly DerivationSchemeParser _derivationSchemeParser;
        public override string ActionId => "SendTransaction";
        public override string Name => "Send Transaction";

        public override string Description =>
            "Send an on chain transaction using NBXplorer";

        public override string ViewPartial => "ViewSendTransactionAction";

        public override string ControllerName => "SendTransaction";

        public SendTransactionDataActionHandler()
        {
        }

        public SendTransactionDataActionHandler(
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

        protected override async Task<TypedActionHandlerResult<BroadcastResult>> Execute(Dictionary<string, object> data, RecipeAction recipeAction,
            SendTransactionData actionData)
        {
            var explorerClient = _nbXplorerClientProvider.GetClient(actionData.CryptoCode);
            var factory = _derivationStrategyFactoryProvider.GetDerivationStrategyFactory(actionData.CryptoCode);
            NBXplorerPublicWallet wallet;
            if (string.IsNullOrEmpty(actionData
                .DerivationStrategy))
            {
                wallet = await _nbXplorerPublicWalletProvider.Get(actionData.CryptoCode,
                    BitcoinAddress.Create(
                        actionData.Address,
                        explorerClient.Network.NBitcoinNetwork));
            }
            else
            {
                wallet = await _nbXplorerPublicWalletProvider.Get(actionData.CryptoCode,
                    _derivationSchemeParser.Parse(factory,
                        actionData.DerivationStrategy));
            }

            var txBuilder =
                await wallet.BuildTransaction(actionData.Outputs.Select(output =>
                    (Money.Parse(InterpolateString(output.Amount, data)),
                        (IDestination) BitcoinAddress.Create(InterpolateString(output.DestinationAddress, data),
                            explorerClient.Network.NBitcoinNetwork))));

            ExtKey key;
            if (!string.IsNullOrEmpty(actionData.MnemonicSeed))
            {
                key = new Mnemonic(actionData.MnemonicSeed).DeriveExtKey(
                    string.IsNullOrEmpty(actionData.Passphrase) ? null : actionData.Passphrase);
            }
            else
            {
                    key = ExtKey.Parse(actionData.WIF, explorerClient.Network.NBitcoinNetwork);
               
            }

            await wallet.SignTransaction(txBuilder, key);
            var tx = txBuilder.BuildTransaction(true);
            var result = await wallet.BroadcastTransaction(tx);
            return new TypedActionHandlerResult<BroadcastResult>()
            {
                Executed = result.Success,
                Data = result,
                Result = $"Tx broadcasted, {(result.Success? "Unsuccessful": "Successful")}, {result.RPCMessage}"
            };
        }
    }
}