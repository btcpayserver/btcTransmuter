using System.Linq;
using BtcTransmuter.Extension.NBXplorer.Services;
using BtcTransmuter.Extension.NBXplorer.Triggers.NBXplorerBalance;
using BtcTransmuter.Tests.Base;

namespace BtcTransmuter.Extension.NBXplorer.Tests
{
    public class NBXplorerBalanceTriggerHandlerTests : BaseTriggerTest<NBXplorerBalanceTriggerHandler,
        NBXplorerBalanceTriggerData,
        NBXplorerBalanceTriggerParameters>
    {
        protected override NBXplorerBalanceTriggerHandler GetTriggerHandlerInstance(params object[] setupArgs)
        {
            if (setupArgs.Any())
            {
                return new NBXplorerBalanceTriggerHandler(
                    (NBXplorerClientProvider) setupArgs.Single(o => o is NBXplorerClientProvider),
                    (NBXplorerPublicWalletProvider) setupArgs.Single(o => o is NBXplorerPublicWalletProvider),
                    (DerivationSchemeParser) setupArgs.Single(o => o is DerivationSchemeParser));
            }

            return new NBXplorerBalanceTriggerHandler();
        }
    }
}