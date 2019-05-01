using System.Linq;
using BtcTransmuter.Extension.NBXplorer.Actions.SendTransaction;
using BtcTransmuter.Extension.NBXplorer.Services;
using BtcTransmuter.Tests.Base;
using NBXplorer.Models;

namespace BtcTransmuter.Extension.NBXplorer.Tests
{
    public class SendTransactionDataActionHandlerTests : BaseActionTest<SendTransactionDataActionHandler,
        SendTransactionData, BroadcastResult>
    {
        protected override SendTransactionDataActionHandler GetActionHandlerInstance(params object[] setupArgs)
        {
            if (setupArgs.Any())
            {
                return new SendTransactionDataActionHandler(
                    (NBXplorerPublicWalletProvider) setupArgs.Single(o => o is NBXplorerPublicWalletProvider),
                    (NBXplorerClientProvider) setupArgs.Single(o => o is NBXplorerClientProvider),
                    (DerivationStrategyFactoryProvider) setupArgs.Single(o => o is DerivationStrategyFactoryProvider),
                    (DerivationSchemeParser) setupArgs.Single(o => o is DerivationSchemeParser));
            }

            return new SendTransactionDataActionHandler();
        }
    }
}