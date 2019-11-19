using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BtcTransmuter.Abstractions.Recipes;
using BtcTransmuter.Abstractions.Triggers;
using BtcTransmuter.Data.Entities;
using BtcTransmuter.Data.Models;
using BtcTransmuter.Extension.Lightning.ExternalServices.NBXplorerWallet;
using BtcTransmuter.Extension.NBXplorer.Services;
using NBXplorer.Models;

namespace BtcTransmuter.Extension.NBXplorer.Triggers.NBXplorerNewTransaction
{
    public class NBXplorerNewTransactionTriggerHandler : BaseTriggerHandler<NBXplorerNewTransactionTriggerData,
        NBXplorerNewTransactionTriggerParameters>
    {
        private readonly IRecipeManager _recipeManager;
        private readonly NBXplorerPublicWalletProvider _nbXplorerPublicWalletProvider;
        private readonly NBXplorerClientProvider _nbXplorerClientProvider;
        private readonly DerivationSchemeParser _derivationSchemeParser;
        public override string TriggerId => NBXplorerNewTransactionTrigger.Id;
        public override string Name => "New Transaction";

        public override string Description =>
            "Trigger a recipe by detecting new transactions made to an xpub or address";

        public override string ViewPartial => "ViewNBXplorerNewTransactionTrigger";
        public override string ControllerName => "NBXplorerNewTransaction";

        public NBXplorerNewTransactionTriggerHandler()
        {
        }

        public NBXplorerNewTransactionTriggerHandler(
            IRecipeManager recipeManager,
            NBXplorerPublicWalletProvider nbXplorerPublicWalletProvider,
            NBXplorerClientProvider nbXplorerClientProvider,
            DerivationSchemeParser derivationSchemeParser)
        {
            _recipeManager = recipeManager;
            _nbXplorerPublicWalletProvider = nbXplorerPublicWalletProvider;
            _nbXplorerClientProvider = nbXplorerClientProvider;
            _derivationSchemeParser = derivationSchemeParser;
        }


        protected override async Task<bool> IsTriggered(ITrigger trigger, RecipeTrigger recipeTrigger,
            NBXplorerNewTransactionTriggerData triggerData,
            NBXplorerNewTransactionTriggerParameters parameters)
        {
            var walletService = new NBXplorerWalletService(recipeTrigger.ExternalService,
                _nbXplorerPublicWalletProvider, _derivationSchemeParser,
                _nbXplorerClientProvider);
            var trackedSource = await walletService.ConstructTrackedSource();

            var walletData = walletService.GetData();
            
            if (!triggerData.CryptoCode.Equals(walletData.CryptoCode,
                StringComparison.InvariantCultureIgnoreCase))
            {
                return false;
            }

            if (triggerData.Event != null && triggerData.Event.TrackedSource == trackedSource)
            {
                return await UpdateTxToRecipeTrigger(triggerData.Event.TransactionData, recipeTrigger,
                    parameters);
            }

            return false;
        }

        private async Task<bool> UpdateTxToRecipeTrigger(TransactionResult transactionResult,
            RecipeTrigger recipeTrigger, NBXplorerNewTransactionTriggerParameters parameters)
        {
            if (parameters.Transactions == null)
            {
                parameters.Transactions = new List<TransactionResult>();
            }
            var matchedIndex = parameters.Transactions.FindIndex(i =>
                i.TransactionHash == transactionResult.TransactionHash);
            var confirmations = transactionResult.Confirmations;
            if (matchedIndex != -1)
            {
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