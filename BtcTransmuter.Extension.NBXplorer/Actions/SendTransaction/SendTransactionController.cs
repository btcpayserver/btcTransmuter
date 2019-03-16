using System;
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
using NBitcoin;
using NBXplorer.DerivationStrategy;
using NBXplorer.Models;

namespace BtcTransmuter.Extension.NBXplorer.Actions.SendTransaction
{
    [Route("nbxplorer-plugin/actions/send-transaction")]
    [Authorize]
    public class SendTransactionController : BaseActionController<SendTransactionController.SendTransactionViewModel,
        SendTransactionData>
    {
        private readonly NBXplorerOptions _nbXplorerOptions;
        private readonly DerivationStrategyFactoryProvider _derivationStrategyFactoryProvider;
        private readonly DerivationSchemeParser _derivationSchemeParser;
        private readonly NBXplorerClientProvider _nbXplorerClientProvider;

        public SendTransactionController(IMemoryCache memoryCache,
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

        protected override async Task<SendTransactionViewModel> BuildViewModel(RecipeAction from)
        {
            var fromData = from.Get<SendTransactionData>();
            return new SendTransactionViewModel
            {
                RecipeId = from.RecipeId,
                CryptoCode = fromData.CryptoCode,
                DerivationStrategy = fromData.DerivationStrategy,
                Passphrase = fromData.Passphrase,
                WIF = fromData.WIF,
                Address = fromData.Address,
                MnemonicSeed = fromData.MnemonicSeed,
                CryptoCodes = new SelectList(_nbXplorerOptions.Cryptos?.ToList() ?? new List<string>(),
                    fromData.CryptoCode)
            };
        }

        protected override async Task<(RecipeAction ToSave, SendTransactionViewModel showViewModel)> BuildModel(
            SendTransactionViewModel viewModel, RecipeAction mainModel)
        {
            viewModel.CryptoCodes = new SelectList(_nbXplorerOptions.Cryptos?.ToList() ?? new List<string>(),
                viewModel.CryptoCode);
            if (viewModel.Action.Equals("add-outgoing", StringComparison.InvariantCultureIgnoreCase))
            {
                viewModel.Outputs.Add(new SendTransactionData.TransactionOutput());
                return (null, viewModel);
            }

            if (viewModel.Action.StartsWith("remove-outgoing", StringComparison.InvariantCultureIgnoreCase))
            {
                var index = int.Parse(viewModel.Action.Substring(viewModel.Action.IndexOf(":") + 1));
                viewModel.Outputs.RemoveAt(index);
                return (null, viewModel);
            }

            if (!viewModel.Outputs.Any())
            {
                ModelState.AddModelError(string.Empty,
                    "Please add at least one transaction output");
            }

            if ((!string.IsNullOrEmpty(viewModel.DerivationStrategy) && !string.IsNullOrEmpty(viewModel.Address)) ||
                string.IsNullOrEmpty(viewModel.DerivationStrategy) && string.IsNullOrEmpty(viewModel.Address))
            {
                ModelState.AddModelError(string.Empty,
                    "Please choose to track either an address OR a derivation scheme");
            }

            var client = _nbXplorerClientProvider.GetClient(viewModel.CryptoCode);

            BitcoinAddress address = null;
            DerivationStrategyBase derivationStrategy = null;
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


            if (string.IsNullOrEmpty(viewModel.WIF) && string.IsNullOrEmpty(viewModel.MnemonicSeed))
            {
                ModelState.AddModelError(string.Empty,
                    "Please enter a mnemonic seed(with or without passphrase) or a private key");
            }
            else
            {
                if (!string.IsNullOrEmpty(viewModel.MnemonicSeed))
                {
                    try
                    {
                        var key = new Mnemonic(viewModel.MnemonicSeed).DeriveExtKey(
                            string.IsNullOrEmpty(viewModel.Passphrase) ? null : viewModel.Passphrase);
                    }
                    catch (Exception e)
                    {
                        ModelState.AddModelError(nameof(SendTransactionViewModel.MnemonicSeed),
                            "Mnemonic seed could not be loaded");
                    }
                }
                else
                {
                    try
                    {
                        var key = ExtKey.Parse(viewModel.WIF, client.Network.NBitcoinNetwork);
                    }
                    catch (Exception e)
                    {
                        ModelState.AddModelError(nameof(SendTransactionViewModel.MnemonicSeed),
                            "WIF could not be loaded");
                    }
                }
            }

            if (ModelState.IsValid)
            {
                mainModel.Set<SendTransactionData>(viewModel);


                if (derivationStrategy != null)
                {
                    client.Track(TrackedSource.Create(derivationStrategy));
                }

                if (address != null)
                {
                    client.Track(TrackedSource.Create(address));
                }

                return (mainModel, null);
            }

            return (null, viewModel);
        }

        public class SendTransactionViewModel : SendTransactionData
        {
            public string Action { get; set; }
            public string RecipeId { get; set; }
            public SelectList CryptoCodes { get; set; }
        }
    }
}