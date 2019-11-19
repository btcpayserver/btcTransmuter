using System;
using System.Threading.Tasks;
using BtcTransmuter.Abstractions.Triggers;
using BtcTransmuter.Data.Entities;
using BtcTransmuter.Extension.Lightning.ExternalServices.NBXplorerWallet;
using BtcTransmuter.Extension.NBXplorer.Services;

namespace BtcTransmuter.Extension.NBXplorer.Triggers.NBXplorerBalance
{
    public class NBXplorerBalanceTriggerHandler : BaseTriggerHandler<NBXplorerBalanceTriggerData,
        NBXplorerBalanceTriggerParameters>
    {
        private readonly NBXplorerClientProvider _nbXplorerClientProvider;
        private readonly NBXplorerPublicWalletProvider _nbXplorerPublicWalletProvider;
        private readonly DerivationSchemeParser _derivationSchemeParser;
        public override string TriggerId => NBXplorerBalanceTrigger.Id;
        public override string Name => "Balance Check";

        public override string Description =>
            "Trigger a recipe by checking if the balance within an address or xpub is within a specific range";

        public override string ViewPartial => "ViewNBXplorerBalanceTrigger";
        public override string ControllerName => "NBXplorerBalance";

        public NBXplorerBalanceTriggerHandler()
        {
        }

        public NBXplorerBalanceTriggerHandler(
            NBXplorerClientProvider nbXplorerClientProvider,
            NBXplorerPublicWalletProvider nbXplorerPublicWalletProvider,
            DerivationSchemeParser derivationSchemeParser)
        {
            _nbXplorerClientProvider = nbXplorerClientProvider;
            _nbXplorerPublicWalletProvider = nbXplorerPublicWalletProvider;
            _derivationSchemeParser = derivationSchemeParser;
        }


        protected override async Task<bool> IsTriggered(ITrigger trigger, RecipeTrigger recipeTrigger,
            NBXplorerBalanceTriggerData triggerData,
            NBXplorerBalanceTriggerParameters parameters)
        {

            var walletService = new NBXplorerWalletService(recipeTrigger.ExternalService,
                _nbXplorerPublicWalletProvider,
                _derivationSchemeParser,
                _nbXplorerClientProvider);

            var walletData = walletService.GetData();
            if (!triggerData.CryptoCode.Equals(walletData.CryptoCode,
                StringComparison.InvariantCultureIgnoreCase))
            {
                return false;
            }

            return await walletService.ConstructTrackedSource() == triggerData.TrackedSource && IsBalanceWithinCriteria(triggerData, parameters);
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
                case BalanceComparer.LessThan:
                    return triggerData.Balance < parameters.Balance;
                case BalanceComparer.GreaterThan:
                    return triggerData.Balance > parameters.Balance;
                case BalanceComparer.Equal:
                    return triggerData.Balance == parameters.Balance;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public override Task<NBXplorerBalanceTriggerData> GetTriggerData(ITrigger trigger)
        {
            if (trigger is NBXplorerBalanceTrigger nbXplorerBalanceTrigger)
            {
               return Task.FromResult(nbXplorerBalanceTrigger.Data);
            }
            return base.GetTriggerData(trigger);
        }
    }
}