using System;
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
using NBitcoin;
using NBXplorer.DerivationStrategy;
using NBXplorer.Models;

namespace BtcTransmuter.Extension.NBXplorer.Triggers.NBXplorerBalance
{
    [Authorize]
    [Route("nbxplorer-plugin/triggers/balance")]
    public class NBXplorerBalanceController : BaseTriggerController<
        NBXplorerBalanceController.NBXplorerBalanceViewModel,
        NBXplorerBalanceTriggerParameters>
    {
        private readonly NBXplorerOptions _options;
        private readonly DerivationStrategyFactoryProvider _derivationStrategyFactoryProvider;
        private readonly NBXplorerClientProvider _nbXplorerClientProvider;
        private readonly DerivationSchemeParser _derivationSchemeParser;

        public NBXplorerBalanceController(IRecipeManager recipeManager, UserManager<User> userManager,
            IMemoryCache memoryCache, NBXplorerOptions options,
            DerivationStrategyFactoryProvider derivationStrategyFactoryProvider,
            NBXplorerClientProvider nbXplorerClientProvider,
            DerivationSchemeParser derivationSchemeParser) : base(recipeManager, userManager,
            memoryCache)
        {
            _options = options;
            _derivationStrategyFactoryProvider = derivationStrategyFactoryProvider;
            _nbXplorerClientProvider = nbXplorerClientProvider;
            _derivationSchemeParser = derivationSchemeParser;
        }

        protected override Task<NBXplorerBalanceViewModel> BuildViewModel(RecipeTrigger data)
        {
            var innerData = data.Get<NBXplorerBalanceTriggerParameters>();

            return Task.FromResult(new NBXplorerBalanceViewModel()
            {
                CryptoCodes = new SelectList(_options.Cryptos?.ToList() ?? new List<string>(),
                    innerData.CryptoCode),

                RecipeId = data.RecipeId,
                CryptoCode = innerData.CryptoCode,
                Address = innerData.Address,
                BalanceComparer = innerData.BalanceComparer,
                BalanceValue = innerData.Balance?.ToUnit(MoneyUnit.BTC)?? 0,
                BalanceMoneyUnit = MoneyUnit.BTC,
                DerivationStrategy = innerData.DerivationStrategy,
            });
        }

        protected override Task<(RecipeTrigger ToSave, NBXplorerBalanceViewModel showViewModel)>
            BuildModel(
                NBXplorerBalanceViewModel viewModel, RecipeTrigger mainModel)
        {
            if (!string.IsNullOrEmpty(viewModel.DerivationStrategy) && !string.IsNullOrEmpty(viewModel.Address))
            {
                ModelState.AddModelError(string.Empty,
                    "Please choose to track either an address OR a derivation scheme");
            }

            BitcoinAddress address = null;
            DerivationStrategyBase derivationStrategy= null;
            if (!string.IsNullOrEmpty(viewModel.Address) && !string.IsNullOrEmpty(viewModel.CryptoCode))
            {
                try
                {
                    var factory =
                        _derivationStrategyFactoryProvider.GetDerivationStrategyFactory(viewModel.CryptoCode);
                    address = BitcoinAddress.Create(viewModel.Address, factory.Network);
                }
                catch (Exception)
                {
                    ModelState.AddModelError(nameof(viewModel.Address), "Invalid Address");
                }
            }

            if (!string.IsNullOrEmpty(viewModel.DerivationStrategy) && !string.IsNullOrEmpty(viewModel.CryptoCode))
            {
                try
                {
                    var factory =
                        _derivationStrategyFactoryProvider.GetDerivationStrategyFactory(viewModel.CryptoCode);

                    derivationStrategy = _derivationSchemeParser.Parse(factory, viewModel.DerivationStrategy);
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
                return Task.FromResult<(RecipeTrigger ToSave, NBXplorerBalanceViewModel showViewModel)>((null,
                    viewModel));
            }

            var recipeTrigger = mainModel;
            recipeTrigger.Set((NBXplorerBalanceTriggerParameters) viewModel);

            var client = _nbXplorerClientProvider.GetClient(viewModel.CryptoCode);
            if (derivationStrategy != null)
            {
                client.Track(TrackedSource.Create(derivationStrategy));
            }

            if (address != null)
            {
                client.Track(TrackedSource.Create(address));
            }
            return Task.FromResult<(RecipeTrigger ToSave, NBXplorerBalanceViewModel showViewModel)>((
                recipeTrigger, null));
        }

        public class NBXplorerBalanceViewModel : NBXplorerBalanceTriggerParameters
        {
            public string RecipeId { get; set; }
            public SelectList CryptoCodes { get; set; }
        }
    }
}