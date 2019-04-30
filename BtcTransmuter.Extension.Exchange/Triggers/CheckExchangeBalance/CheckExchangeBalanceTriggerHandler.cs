using System;
using System.Threading.Tasks;
using BtcTransmuter.Abstractions.Triggers;
using BtcTransmuter.Data.Entities;

namespace BtcTransmuter.Extension.Exchange.Triggers.CheckExchangeBalance
{
    public class CheckExchangeBalanceTriggerHandler : BaseTriggerHandler<CheckExchangeBalanceTriggerData,
        CheckExchangeBalanceTriggerParameters>
    {
        public override string TriggerId => CheckExchangeBalanceTrigger.Id;
        public override string Name => "Exchange Balance Check";

        public override string Description =>
            "Trigger a recipe by checking if the balance of an asset on an exchange ";

        public override string ViewPartial => "ViewCheckExchangeBalanceTrigger";
        public override string ControllerName => "CheckExchangeBalance";


        protected override Task<bool> IsTriggered(ITrigger trigger, RecipeTrigger recipeTrigger,
            CheckExchangeBalanceTriggerData triggerData,
            CheckExchangeBalanceTriggerParameters parameters)
        {
            return Task.FromResult(recipeTrigger.ExternalServiceId.Equals(triggerData.ExternalServiceId) &&
                                   triggerData.Asset.Equals(parameters.Asset, StringComparison.InvariantCultureIgnoreCase) &&
                                   IsBalanceWithinCriteria(triggerData, parameters));
        }

        private static bool IsBalanceWithinCriteria(CheckExchangeBalanceTriggerData triggerData,
            CheckExchangeBalanceTriggerParameters parameters)
        {
            switch (parameters.BalanceComparer)
            {
                case BalanceComparer.LessThanOrEqual:
                    return triggerData.Balance <= parameters.BalanceValue;
                case BalanceComparer.GreaterThanOrEqual:
                    return triggerData.Balance >= parameters.BalanceValue;
                case BalanceComparer.LessThan:
                    return triggerData.Balance < parameters.BalanceValue;
                case BalanceComparer.GreaterThan:
                    return triggerData.Balance > parameters.BalanceValue;
                case BalanceComparer.Equal:
                    return triggerData.Balance == parameters.BalanceValue;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public override Task<CheckExchangeBalanceTriggerData> GetTriggerData(ITrigger trigger)
        {
            if (trigger is CheckExchangeBalanceTrigger CheckExchangeBalanceTrigger)
            {
                return Task.FromResult(CheckExchangeBalanceTrigger.Data);
            }

            return base.GetTriggerData(trigger);
        }
    }
}