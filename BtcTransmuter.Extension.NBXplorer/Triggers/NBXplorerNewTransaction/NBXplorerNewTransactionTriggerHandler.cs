using System;
using System.Threading.Tasks;
using BtcTransmuter.Abstractions.Triggers;
using BtcTransmuter.Data.Entities;
using BtcTransmuter.Extension.NBXplorer.Services;
using NBitcoin;
using NBXplorer.DerivationStrategy;
using NBXplorer.Models;

namespace BtcTransmuter.Extension.NBXplorer.Triggers.NBXplorerNewTransaction
{
    public class NBXplorerNewTransactionTriggerHandler : BaseTriggerHandler<NBXplorerNewTransactionTriggerData,
        NBXplorerNewTransactionTriggerParameters>
    {
        private readonly DerivationStrategyFactoryProvider _derivationStrategyFactoryProvider;
        private readonly DerivationSchemeParser _derivationSchemeParser;
        public override string TriggerId => new NBXplorerNewTransactionTrigger().Id;
        public override string Name => "New Transaction";

        public override string Description =>
            "Trigger a recipe by detecting new transactions made to an xpub or address";

        public override string ViewPartial => "ViewNBXplorerNewTransactionTrigger";
        protected override string ControllerName => "NBXplorerNewTransaction";

        public NBXplorerNewTransactionTriggerHandler()
        {
        }

        public NBXplorerNewTransactionTriggerHandler(
            DerivationStrategyFactoryProvider derivationStrategyFactoryProvider,
            DerivationSchemeParser derivationSchemeParser)
        {
            _derivationStrategyFactoryProvider = derivationStrategyFactoryProvider;
            _derivationSchemeParser = derivationSchemeParser;
        }


        protected override async Task<bool> IsTriggered(ITrigger trigger, RecipeTrigger recipeTrigger,
            NBXplorerNewTransactionTriggerData triggerData,
            NBXplorerNewTransactionTriggerParameters parameters)
        {
            if (triggerData.CryptoCode.Equals(parameters.CryptoCode,
                StringComparison.InvariantCultureIgnoreCase))
            {
                return false;
            }

            switch (triggerData.Event.TrackedSource)
            {
                case AddressTrackedSource addressTrackedSource:
                    if (string.IsNullOrEmpty(parameters.Address))
                    {
                        return false;
                    }

                    return addressTrackedSource.Address.ToString()
                        .Equals(parameters.Address, StringComparison.InvariantCultureIgnoreCase);

                case DerivationSchemeTrackedSource derivationSchemeTrackedSource:
                    if (string.IsNullOrEmpty(parameters.DerivationStrategy))
                    {
                        return false;
                    }

                    var factory =
                        _derivationStrategyFactoryProvider.GetDerivationStrategyFactory(parameters.CryptoCode);

                    return derivationSchemeTrackedSource.DerivationStrategy ==
                           _derivationSchemeParser.Parse(factory, parameters.DerivationStrategy);
            }

            return false;
        }
    }
}