using System.Linq;
using BtcTransmuter.Extension.NBXplorer.Actions.NBXplorerGetBalance;
using BtcTransmuter.Extension.NBXplorer.Services;
using BtcTransmuter.Tests.Base;
using NBitcoin;

namespace BtcTransmuter.Extension.NBXplorer.Tests
{
    public class NBXplorerGetBalanceDataActionHandlerTests : BaseActionTest<NBXplorerGetBalanceDataActionHandler,
        NBXplorerGetBalanceData, Money>
    {
        protected override NBXplorerGetBalanceDataActionHandler GetActionHandlerInstance(params object[] setupArgs)
        {
            if (setupArgs.Any())
            {
                return new NBXplorerGetBalanceDataActionHandler(
                    (NBXplorerClientProvider) setupArgs.Single(o => o is NBXplorerClientProvider),
                    (NBXplorerPublicWalletProvider) setupArgs.Single(o => o is NBXplorerPublicWalletProvider),
                    (DerivationSchemeParser) setupArgs.Single(o => o is DerivationSchemeParser));
            }

            return new NBXplorerGetBalanceDataActionHandler();
        }
    }
}