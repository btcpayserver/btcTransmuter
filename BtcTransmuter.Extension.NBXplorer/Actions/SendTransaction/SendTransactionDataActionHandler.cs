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
using NBXplorer.Models;

namespace BtcTransmuter.Extension.NBXplorer.Actions.SendTransaction
{
	public class SendTransactionDataActionHandler : BaseActionHandler<SendTransactionData, BroadcastResult>
	{
		private readonly NBXplorerPublicWalletProvider _nbXplorerPublicWalletProvider;
		private readonly NBXplorerClientProvider _nbXplorerClientProvider;
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
			DerivationSchemeParser derivationSchemeParser)
		{
			_nbXplorerPublicWalletProvider = nbXplorerPublicWalletProvider;
			_nbXplorerClientProvider = nbXplorerClientProvider;
			_derivationSchemeParser = derivationSchemeParser;
		}

		protected override async Task<TypedActionHandlerResult<BroadcastResult>> Execute(
			Dictionary<string, object> data, RecipeAction recipeAction,
			SendTransactionData actionData)
		{
			var externalService = await recipeAction.GetExternalService();
			var walletService = new NBXplorerWalletService(externalService, _nbXplorerPublicWalletProvider,
				_derivationSchemeParser, _nbXplorerClientProvider);

			var wallet = await walletService.ConstructClient();
			var walletData = walletService.GetData();
			var explorerClient = _nbXplorerClientProvider.GetClient(walletData.CryptoCode);
			var tx = await wallet.BuildTransaction(actionData.Outputs.Select(output =>
					(Money.Parse(InterpolateString(output.Amount, data)),
						(IDestination) BitcoinAddress.Create(InterpolateString(output.DestinationAddress, data),
							explorerClient.Network.NBitcoinNetwork), output.SubtractFeesFromOutput)),
				walletData.PrivateKeys,
				actionData.FeeSatoshiPerByte != null
					? new FeeRate(Money.Satoshis(actionData.FeeSatoshiPerByte.Value), 1)
					: null, actionData.Fee.HasValue ? new Money(actionData.Fee.Value, MoneyUnit.BTC) : null);
			var result = await wallet.BroadcastTransaction(tx);

			return new NBXplorerActionHandlerResult<BroadcastResult>(
				_nbXplorerClientProvider.GetClient(walletData.CryptoCode))
			{
				Executed = result.Success,
				TypedData = result,
				Result =
					$"Tx broadcasted, {(result.Success ? "Successful" : "Unsuccessful")}, {result.RPCMessage}, {tx}"
			};
		}
	}
}