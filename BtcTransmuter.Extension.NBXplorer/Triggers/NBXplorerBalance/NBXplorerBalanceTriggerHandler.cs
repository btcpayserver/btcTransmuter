using System;
using System.Threading.Tasks;
using BtcTransmuter.Abstractions.Triggers;
using BtcTransmuter.Data.Entities;
using BtcTransmuter.Extension.NBXplorer.Services;
using NBXplorer.Models;

namespace BtcTransmuter.Extension.NBXplorer.Triggers.NBXplorerBalance
{
    public class NBXplorerBalanceTriggerHandler : BaseTriggerHandler<NBXplorerBalanceTriggerData,
        NBXplorerBalanceTriggerParameters>
    {
        private readonly DerivationStrategyFactoryProvider _derivationStrategyFactoryProvider;
        private readonly DerivationSchemeParser _derivationSchemeParser;
        public override string TriggerId => NBXplorerBalanceTrigger.Id;
        public override string Name => "Balance Check";

        public override string Description =>
            "Trigger a recipe by checking if the balance within an address or xpub is within a specific range";

        public override string ViewPartial => "ViewNBXplorerBalanceTrigger";
        protected override string ControllerName => "NBXplorerBalance";

        public NBXplorerBalanceTriggerHandler()
        {
        }

        public NBXplorerBalanceTriggerHandler(
            DerivationStrategyFactoryProvider derivationStrategyFactoryProvider,
            DerivationSchemeParser derivationSchemeParser)
        {
            _derivationStrategyFactoryProvider = derivationStrategyFactoryProvider;
            _derivationSchemeParser = derivationSchemeParser;
        }


        protected override Task<bool> IsTriggered(ITrigger trigger, RecipeTrigger recipeTrigger,
            NBXplorerBalanceTriggerData triggerData,
            NBXplorerBalanceTriggerParameters parameters)
        {
            if (!triggerData.CryptoCode.Equals(parameters.CryptoCode,
                StringComparison.InvariantCultureIgnoreCase))
            {
                return Task.FromResult(false);
            }

            switch (triggerData.TrackedSource)
            {
                case AddressTrackedSource addressTrackedSource:
                    if (string.IsNullOrEmpty(parameters.Address))
                    {
                        return Task.FromResult(false);
                    }

                    if (addressTrackedSource.Address.ToString()
                        .Equals(parameters.Address, StringComparison.InvariantCultureIgnoreCase))
                    {
                        return Task.FromResult(IsBalanceWithinCriteria(triggerData, parameters));
                    }

                    break;
                case DerivationSchemeTrackedSource derivationSchemeTrackedSource:
                    if (string.IsNullOrEmpty(parameters.DerivationStrategy))
                    {
                        return Task.FromResult(false);
                    }

                    var factory =
                        _derivationStrategyFactoryProvider.GetDerivationStrategyFactory(parameters.CryptoCode);

                    if (derivationSchemeTrackedSource.DerivationStrategy ==
                        _derivationSchemeParser.Parse(factory, parameters.DerivationStrategy))
                    {
                        return Task.FromResult(IsBalanceWithinCriteria(triggerData, parameters));
                    }

                    break;
            }

            return Task.FromResult(false);
        }

        private static bool IsBalanceWithinCriteria(NBXplorerBalanceTriggerData triggerData,
            NBXplorerBalanceTriggerParameters parameters)
        {
            switch (parameters.BalanceComparer)
            {
                case BalanceComparer.LessThanOrEqual:
                    return triggerData.Balance <= parameters.Balance;
                case BalanceComparer.GreaterThanOrEqual:
                    return triggerData.Balance >= parameters.Balance;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}