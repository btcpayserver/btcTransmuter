using System;
using System.Threading.Tasks;
using BtcTransmuter.Abstractions.Triggers;
using BtcTransmuter.Data.Entities;
using BtcTransmuter.Extension.NBXplorer.Services;
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


        protected override Task<bool> IsTriggered(ITrigger trigger, RecipeTrigger recipeTrigger,
            NBXplorerNewTransactionTriggerData triggerData,
            NBXplorerNewTransactionTriggerParameters parameters)
        {
            if (triggerData.CryptoCode.Equals(parameters.CryptoCode,
                StringComparison.InvariantCultureIgnoreCase))
            {
                return Task.FromResult(false);
            }

            switch (triggerData.Event.TrackedSource)
            {
                case AddressTrackedSource addressTrackedSource:
                    if (string.IsNullOrEmpty(parameters.Address))
                    {
                        return Task.FromResult(false);
                    }

                    return Task.FromResult(addressTrackedSource.Address.ToString()
                        .Equals(parameters.Address, StringComparison.InvariantCultureIgnoreCase));

                case DerivationSchemeTrackedSource derivationSchemeTrackedSource:
                    if (string.IsNullOrEmpty(parameters.DerivationStrategy))
                    {
                        return Task.FromResult(false);
                    }

                    var factory =
                        _derivationStrategyFactoryProvider.GetDerivationStrategyFactory(parameters.CryptoCode);

                    return Task.FromResult(derivationSchemeTrackedSource.DerivationStrategy ==
                                           _derivationSchemeParser.Parse(factory, parameters.DerivationStrategy));
            }

            return Task.FromResult(false);
        }
    }
}