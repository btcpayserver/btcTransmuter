using System.Linq;
using BtcTransmuter.Abstractions.Recipes;
using BtcTransmuter.Extension.NBXplorer.Services;
using BtcTransmuter.Extension.NBXplorer.Triggers.NBXplorerNewTransaction;
using BtcTransmuter.Tests.Base;

namespace BtcTransmuter.Extension.NBXplorer.Tests
{
    public class NBXplorerNewTransactionTriggerHandlerTests : BaseTriggerTest<NBXplorerNewTransactionTriggerHandler,
        NBXplorerNewTransactionTriggerData,
        NBXplorerNewTransactionTriggerParameters>
    {
        protected override NBXplorerNewTransactionTriggerHandler GetTriggerHandlerInstance(params object[] setupArgs)
        {
            if (setupArgs.Any())
            {
                return new NBXplorerNewTransactionTriggerHandler(
                    (IRecipeManager) setupArgs.Single(o => o is IRecipeManager),
                    (NBXplorerPublicWalletProvider) setupArgs.Single(o => o is NBXplorerPublicWalletProvider),
                    (NBXplorerClientProvider) setupArgs.Single(o => o is NBXplorerClientProvider),
                    (DerivationSchemeParser) setupArgs.Single(o => o is DerivationSchemeParser));
            }

            return new NBXplorerNewTransactionTriggerHandler();
        }
    }
}