using System;
using System.Linq;
using System.Threading.Tasks;
using BtcTransmuter.Abstractions.Actions;
using BtcTransmuter.Abstractions.Recipes;
using BtcTransmuter.Abstractions.Triggers;
using BtcTransmuter.Data.Entities;
using BtcTransmuter.Data.Models;
using BtcTransmuter.Extension.NBXplorer.Services;
using NBXplorer.Models;

namespace BtcTransmuter.Extension.NBXplorer.Triggers.NBXplorerNewTransaction
{
    public class NBXplorerNewTransactionTriggerHandler : BaseTriggerHandler<NBXplorerNewTransactionTriggerData,
        NBXplorerNewTransactionTriggerParameters>
    {
        private readonly IRecipeManager _recipeManager;
        private readonly DerivationStrategyFactoryProvider _derivationStrategyFactoryProvider;
        private readonly IActionDispatcher _actionDispatcher;
        private readonly DerivationSchemeParser _derivationSchemeParser;
        public override string TriggerId => NBXplorerNewTransactionTrigger.Id;
        public override string Name => "New Transaction";

        public override string Description =>
            "Trigger a recipe by detecting new transactions made to an xpub or address";

        public override string ViewPartial => "ViewNBXplorerNewTransactionTrigger";
        protected override string ControllerName => "NBXplorerNewTransaction";

        public NBXplorerNewTransactionTriggerHandler()
        {
        }

        public NBXplorerNewTransactionTriggerHandler(
            IRecipeManager recipeManager,
            DerivationStrategyFactoryProvider derivationStrategyFactoryProvider,
            DerivationSchemeParser derivationSchemeParser)
        {
            _recipeManager = recipeManager;
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

            if (triggerData.Event != null)
            {
                switch (triggerData.Event.TrackedSource)
                {
                    case AddressTrackedSource addressTrackedSource:
                        if (string.IsNullOrEmpty(parameters.Address))
                        {
                            return false;
                        }

                        if (addressTrackedSource.Address.ToString()
                            .Equals(parameters.Address, StringComparison.InvariantCultureIgnoreCase))
                        {
                            return await UpdateTxToRecipeTrigger(triggerData.Event.TransactionData, recipeTrigger,
                                parameters);
                        }

                        break;
                    case DerivationSchemeTrackedSource derivationSchemeTrackedSource:
                        if (string.IsNullOrEmpty(parameters.DerivationStrategy))
                        {
                            return false;
                        }

                        var factory =
                            _derivationStrategyFactoryProvider.GetDerivationStrategyFactory(parameters.CryptoCode);

                        if (derivationSchemeTrackedSource.DerivationStrategy ==
                            _derivationSchemeParser.Parse(factory, parameters.DerivationStrategy))
                        {
                            return await UpdateTxToRecipeTrigger(triggerData.Event.TransactionData, recipeTrigger,
                                parameters);
                        }

                        break;
                }
            }

            return false;
        }

        private async Task<bool> UpdateTxToRecipeTrigger(TransactionResult transactionResult,
            RecipeTrigger recipeTrigger, NBXplorerNewTransactionTriggerParameters parameters)
        {
            var matchedIndex = parameters.Transactions.FindIndex(i =>
                i.TransactionHash == transactionResult.TransactionHash);
            var confirmations = transactionResult.Confirmations;
            if (matchedIndex != -1)
            {
                confirmations = parameters.Transactions.ElementAt(matchedIndex).Confirmations;
                parameters.Transactions.RemoveAt(matchedIndex);
            }

            var result = parameters.ConfirmationsRequired < confirmations;
            if (!result)
            {
                parameters.Transactions.Add(transactionResult);
            }

            if (!result || matchedIndex != -1)
            {
                recipeTrigger.Set(parameters);
                await _recipeManager.AddOrUpdateRecipeTrigger(recipeTrigger);
            }

            return result;
        }
    }
}