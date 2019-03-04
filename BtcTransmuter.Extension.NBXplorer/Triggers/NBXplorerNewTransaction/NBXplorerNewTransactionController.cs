using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BtcTransmuter.Abstractions.Recipes;
using BtcTransmuter.Abstractions.Triggers;
using BtcTransmuter.Data.Entities;
using BtcTransmuter.Data.Models;
using BtcTransmuter.Extension.NBXplorer.Models;
using BtcTransmuter.Extension.NBXplorer.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using NBXplorer.DerivationStrategy;

namespace BtcTransmuter.Extension.NBXplorer.Triggers.NBXplorerNewTransaction
{
    [Authorize]
    [Route("nbxplorer-plugin/triggers/new-transaction")]
    public class NBXplorerNewTransactionController : BaseTriggerController<
        NBXplorerNewTransactionController.NBXplorerNewTransactionViewModel,
        NBXplorerNewTransactionTriggerParameters>
    {
        private readonly NBXplorerOptions _options;
        private readonly DerivationStrategyFactoryProvider _derivationStrategyFactoryProvider;
        private readonly DerivationSchemeParser _derivationSchemeParser;

        public NBXplorerNewTransactionController(IRecipeManager recipeManager, UserManager<User> userManager,
            IMemoryCache memoryCache, NBXplorerOptions options,
            DerivationStrategyFactoryProvider derivationStrategyFactoryProvider,DerivationSchemeParser  derivationSchemeParser) : base(recipeManager, userManager,
            memoryCache)
        {
            _options = options;
            _derivationStrategyFactoryProvider = derivationStrategyFactoryProvider;
            _derivationSchemeParser = derivationSchemeParser;
        }

        protected override async Task<NBXplorerNewTransactionViewModel> BuildViewModel(RecipeTrigger data)
        {
            var innerData = data.Get<NBXplorerNewTransactionTriggerParameters>();

            return new NBXplorerNewTransactionViewModel()
            {
                CryptoCodes = new SelectList(_options.Cryptos?.ToList() ?? new List<string>(),
                    innerData.CryptoCode),

                RecipeId = data.RecipeId,
                CryptoCode = innerData.CryptoCode,
                Address = innerData.Address,
                DerivationStrategy = innerData.DerivationStrategy,
            };
        }

        protected override async Task<(RecipeTrigger ToSave, NBXplorerNewTransactionViewModel showViewModel)>
            BuildModel(
                NBXplorerNewTransactionViewModel viewModel, RecipeTrigger mainModel)
        {
            if (!string.IsNullOrEmpty(viewModel.DerivationStrategy) && !string.IsNullOrEmpty(viewModel.Address))
            {
                ModelState.AddModelError(string.Empty,
                    "Please choose to track either an address OR a derivation scheme");
            }

            if (!string.IsNullOrEmpty(viewModel.DerivationStrategy) && !string.IsNullOrEmpty(viewModel.CryptoCode))
            {
                try
                {
                    var factory =
                        _derivationStrategyFactoryProvider.GetDerivationStrategyFactory(viewModel.CryptoCode);

                    _derivationSchemeParser.Parse(factory, viewModel.DerivationStrategy);
                }
                catch
                {
                    ModelState.AddModelError(nameof(viewModel.DerivationStrategy), "Invalid Derivation Scheme");
                }
            }

            if (!ModelState.IsValid)
            {
                viewModel.CryptoCodes = new SelectList(_options.Cryptos?.ToList() ?? new List<string>(),
                    viewModel.CryptoCode);
                return (null, viewModel);
            }

            var recipeTrigger = mainModel;
            recipeTrigger.Set((NBXplorerNewTransactionTriggerParameters) viewModel);
            return (recipeTrigger, null);
        }

        public class NBXplorerNewTransactionViewModel : NBXplorerNewTransactionTriggerParameters
        {
            public string RecipeId { get; set; }
            public SelectList CryptoCodes { get; set; }
        }
    }
}