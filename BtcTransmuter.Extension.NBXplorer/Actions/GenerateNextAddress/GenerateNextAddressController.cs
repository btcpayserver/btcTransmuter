using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BtcTransmuter.Abstractions.Actions;
using BtcTransmuter.Abstractions.Recipes;
using BtcTransmuter.Data.Entities;
using BtcTransmuter.Data.Models;
using BtcTransmuter.Extension.NBXplorer.Models;
using BtcTransmuter.Extension.NBXplorer.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Caching.Memory;
using NBXplorer.DerivationStrategy;
using NBXplorer.Models;

namespace BtcTransmuter.Extension.NBXplorer.Actions.GenerateNextAddress
{
    [Route("nbxplorer-plugin/actions/generate-next-address")]
    [Authorize]
    public class GenerateNextAddressController : BaseActionController<
        GenerateNextAddressController.GenerateNextAddressViewModel,
        GenerateNextAddressData>
    {
        private readonly NBXplorerOptions _nbXplorerOptions;
        private readonly DerivationStrategyFactoryProvider _derivationStrategyFactoryProvider;
        private readonly DerivationSchemeParser _derivationSchemeParser;
        private readonly NBXplorerClientProvider _nbXplorerClientProvider;

        public GenerateNextAddressController(IMemoryCache memoryCache,
            UserManager<User> userManager,
            NBXplorerOptions nbXplorerOptions,
            IRecipeManager recipeManager, DerivationStrategyFactoryProvider derivationStrategyFactoryProvider,
            DerivationSchemeParser derivationSchemeParser, NBXplorerClientProvider nbXplorerClientProvider) : base(
            memoryCache,
            userManager, recipeManager)
        {
            _nbXplorerOptions = nbXplorerOptions;
            _derivationStrategyFactoryProvider = derivationStrategyFactoryProvider;
            _derivationSchemeParser = derivationSchemeParser;
            _nbXplorerClientProvider = nbXplorerClientProvider;
        }

        protected override Task<GenerateNextAddressViewModel> BuildViewModel(RecipeAction from)
        {
            var fromData = from.Get<GenerateNextAddressData>();
            return Task.FromResult(new GenerateNextAddressViewModel
            {
                RecipeId = from.RecipeId,
                CryptoCode = fromData.CryptoCode,
                DerivationStrategy = fromData.DerivationStrategy,
                CryptoCodes = new SelectList(_nbXplorerOptions.Cryptos?.ToList() ?? new List<string>(),
                    fromData.CryptoCode)
            });
        }

        protected override async Task<(RecipeAction ToSave, GenerateNextAddressViewModel showViewModel)> BuildModel(
            GenerateNextAddressViewModel viewModel, RecipeAction mainModel)
        {
            viewModel.CryptoCodes = new SelectList(_nbXplorerOptions.Cryptos?.ToList() ?? new List<string>(),
                viewModel.CryptoCode);


            DerivationStrategyBase derivationStrategy = null;

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

            if (ModelState.IsValid)
            {
                mainModel.Set<GenerateNextAddressData>(viewModel);
                var client = _nbXplorerClientProvider.GetClient(viewModel.CryptoCode);
                client.Track(TrackedSource.Create(derivationStrategy));
                return (mainModel, null);
            }

            return (null, viewModel);
        }

        public class GenerateNextAddressViewModel : GenerateNextAddressData, IActionViewModel
        {
            public string RecipeId { get; set; }
            public string RecipeActionIdInGroupBeforeThisOne { get; set; }
            public SelectList CryptoCodes { get; set; }
        }
    }
}