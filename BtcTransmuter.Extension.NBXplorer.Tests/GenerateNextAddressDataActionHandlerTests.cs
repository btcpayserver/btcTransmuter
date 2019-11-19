using System;
using System.Linq;
using BtcTransmuter.Extension.NBXplorer.Actions.GenerateNextAddress;
using BtcTransmuter.Extension.NBXplorer.Services;
using BtcTransmuter.Tests.Base;
using NBitcoin;
using Xunit;

namespace BtcTransmuter.Extension.NBXplorer.Tests
{
    public class GenerateNextAddressDataActionHandlerTests : BaseActionTest<GenerateNextAddressDataActionHandler,
        GenerateNextAddressData, BitcoinAddress>
    {
        protected override GenerateNextAddressDataActionHandler GetActionHandlerInstance(params object[] setupArgs)
        {
            if (setupArgs.Any())
            {
                return new GenerateNextAddressDataActionHandler(
                    (NBXplorerClientProvider) setupArgs.Single(o => o is NBXplorerClientProvider),
                    (NBXplorerPublicWalletProvider) setupArgs.Single(o => o is NBXplorerPublicWalletProvider),
                    (DerivationSchemeParser) setupArgs.Single(o => o is DerivationSchemeParser));
            }

            return new GenerateNextAddressDataActionHandler();
        }
    }
}