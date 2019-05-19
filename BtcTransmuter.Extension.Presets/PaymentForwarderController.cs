using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using BtcTransmuter.Abstractions.Extensions;
using BtcTransmuter.Abstractions.ExternalServices;
using BtcTransmuter.Abstractions.Recipes;
using BtcTransmuter.Data.Entities;
using BtcTransmuter.Extension.Lightning.ExternalServices.NBXplorerWallet;
using BtcTransmuter.Extension.NBXplorer.Actions.GenerateNextAddress;
using BtcTransmuter.Extension.NBXplorer.Actions.SendTransaction;
using BtcTransmuter.Extension.NBXplorer.Models;
using BtcTransmuter.Extension.NBXplorer.Services;
using BtcTransmuter.Extension.NBXplorer.Triggers.NBXplorerBalance;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore.Internal;
using NBitcoin;
using NBXplorer.DerivationStrategy;
using Newtonsoft.Json;

namespace BtcTransmuter.Extension.Presets
{
    [Route("presets-plugin/presets/PaymentForwarder")]
    [Authorize]
    public class PaymentForwarderController : Controller, ITransmuterPreset
    {
        private readonly IExternalServiceManager _externalServiceManager;
        private readonly UserManager<User> _userManager;
        private readonly NBXplorerOptions _nbXplorerOptions;
        private readonly DerivationStrategyFactoryProvider _derivationStrategyFactoryProvider;
        private readonly NBXplorerPublicWalletProvider _nbXplorerPublicWalletProvider;
        private readonly DerivationSchemeParser _derivationSchemeParser;
        private readonly NBXplorerClientProvider _nbXplorerClientProvider;
        private readonly IRecipeManager _recipeManager;
        public string Id { get; } = "PaymentForwarder";
        public string Name { get; } = "On-chain Forwarder";
        public string Description { get; } = "Forward funds from a wallet elsewhere";

        public PaymentForwarderController(
            IExternalServiceManager externalServiceManager,
            UserManager<User> userManager,
            NBXplorerOptions nbXplorerOptions,
            DerivationStrategyFactoryProvider derivationStrategyFactoryProvider,
            NBXplorerPublicWalletProvider nbXplorerPublicWalletProvider,
            DerivationSchemeParser derivationSchemeParser,
            NBXplorerClientProvider nbXplorerClientProvider,
            IRecipeManager recipeManager)
        {
            _externalServiceManager = externalServiceManager;
            _userManager = userManager;
            _nbXplorerOptions = nbXplorerOptions;
            _derivationStrategyFactoryProvider = derivationStrategyFactoryProvider;
            _nbXplorerPublicWalletProvider = nbXplorerPublicWalletProvider;
            _derivationSchemeParser = derivationSchemeParser;
            _nbXplorerClientProvider = nbXplorerClientProvider;
            _recipeManager = recipeManager;
        }

        public (string ControllerName, string ActionName) GetLink()
        {
            return (Id, nameof(Create));
        }

        [HttpGet("create")]
        public async Task<IActionResult> Create()
        {
            var services = await _externalServiceManager.GetExternalServicesData(new ExternalServicesDataQuery()
            {
                UserId = _userManager.GetUserId(User),
                Type = new[] {NBXplorerWalletService.NBXplorerWalletServiceType}
            });
            if (services.Any())
            {
                var newServices = services.ToList();
                newServices.Insert(0, new ExternalServiceData()
                {
                    Id = null,
                    Name = "None"
                });
                services = newServices;
            }

            return View(new CreatePaymentForwarderViewModel()
            {
                Services = new SelectList(services, nameof(ExternalServiceData.Id), nameof(ExternalServiceData.Name)),
                CryptoCodes = new SelectList(_nbXplorerOptions.Cryptos?.ToList() ?? new List<string>()),
                PaymentDestinations = new List<CreatePaymentForwarderViewModel.PaymentDestination>()
                {
                    new CreatePaymentForwarderViewModel.PaymentDestination()
                },
                GenerateSourceWallet = !services.Any()
            });
        }

        [HttpPost("create")]
        public async Task<IActionResult> Create(CreatePaymentForwarderViewModel viewModel)
        {
            var services = await _externalServiceManager.GetExternalServicesData(new ExternalServicesDataQuery()
            {
                UserId = _userManager.GetUserId(User),
                Type = new[] {NBXplorerWalletService.NBXplorerWalletServiceType}
            });
            if (services.Any())
            {
                var newServices = services.ToList();
                newServices.Insert(0, new ExternalServiceData()
                {
                    Id = null,
                    Name = "None"
                });
                services = newServices;
            }

            viewModel.Services =
                new SelectList(services, nameof(ExternalServiceData.Id), nameof(ExternalServiceData.Name));
            viewModel.CryptoCodes = new SelectList(_nbXplorerOptions.Cryptos?.ToList() ?? new List<string>(),
                viewModel.CryptoCode);
            viewModel.PaymentDestinations = viewModel.PaymentDestinations ??
                                            new List<CreatePaymentForwarderViewModel.PaymentDestination>();

            if (!string.IsNullOrEmpty(viewModel.Action))
            {
                if (viewModel.Action == "add-destination")
                {
                    viewModel.PaymentDestinations.Add(new CreatePaymentForwarderViewModel.PaymentDestination());
                    return View(viewModel);
                }

                if (viewModel.Action.StartsWith("remove-destination", StringComparison.InvariantCultureIgnoreCase))
                {
                    var index = int.Parse(viewModel.Action.Substring(viewModel.Action.IndexOf(":") + 1));
                    viewModel.PaymentDestinations.RemoveAt(index);
                    return View(viewModel);
                }
            }

            if ((string.IsNullOrEmpty(viewModel.SelectedSourceWalletExternalServiceId) &&
                 !viewModel.GenerateSourceWallet) ||
                !string.IsNullOrEmpty(viewModel.SelectedSourceWalletExternalServiceId) &&
                viewModel.GenerateSourceWallet)
            {
                if (viewModel.Services.Items.Any())
                {
                    ModelState.AddModelError(nameof(viewModel.SelectedSourceWalletExternalServiceId),
                        "Please select a source nbxplorer wallet OR check the generate wallet checkbox");
                }

                ModelState.AddModelError(nameof(viewModel.GenerateSourceWallet),
                    "Please select a source nbxplorer wallet OR check the generate wallet checkbox");
            }
            else if (!string.IsNullOrEmpty(viewModel.SelectedSourceWalletExternalServiceId))
            {
                var service = services.SingleOrDefault(data =>
                    data.Id == viewModel.SelectedSourceWalletExternalServiceId);
                if (service == null)
                {
                    viewModel.AddModelError(
                        model => model.SelectedSourceWalletExternalServiceId,
                        "Wallet chosen is wrong... ヽ༼ ಠ益ಠ ༽ﾉ ", ModelState);
                }
                else
                {
                    var data = new NBXplorerWalletService(service, _nbXplorerPublicWalletProvider,
                        _derivationSchemeParser,
                        _derivationStrategyFactoryProvider, _nbXplorerClientProvider).GetData();
                    if (data.CryptoCode != viewModel.CryptoCode)
                    {
                        viewModel.AddModelError(
                            model => model.SelectedSourceWalletExternalServiceId,
                            "Wallet chosen not the same crypto", ModelState);
                    }
                    else if (!data.PrivateKeys.Any())
                    {
                        viewModel.AddModelError(
                            model => model.SelectedSourceWalletExternalServiceId,
                            "Wallet chosen has so signing keys which would make forwarding txs impossible", ModelState);
                    }
                }
            }

            if (!viewModel.PaymentDestinations.Any())
            {
                ModelState.AddModelError(string.Empty,
                    "Please add at least one transaction destination");
            }
            else
            {
                var totalSumIssue = viewModel.PaymentDestinations.Sum(output => output.AmountPercentage) > 100;


                var subtractFeesOutputs = viewModel.PaymentDestinations.Select((output, i) => (output, i))
                    .Where(tuple => tuple.Item1.SubtractFeesFromOutput);

                if (subtractFeesOutputs.Count() > 1)
                {
                    foreach (var subtractFeesOutput in subtractFeesOutputs)
                    {
                        viewModel.AddModelError(
                            model => model.PaymentDestinations[subtractFeesOutput.Item2].SubtractFeesFromOutput,
                            "You can only subtract fees from one destination", ModelState);
                    }
                }

                for (var index = 0; index < viewModel.PaymentDestinations.Count; index++)
                {
                    if (totalSumIssue)
                    {
                        viewModel.AddModelError(
                            model => model.PaymentDestinations[index].AmountPercentage,
                            "Your total amounts across all outputs exceeds 100% We're not a central bank and can't print more money than you own, sorry.",
                            ModelState);
                    }

                    var viewModelPaymentDestination = viewModel.PaymentDestinations[index];

                    var check =
                        (string.IsNullOrEmpty(viewModelPaymentDestination.DerivationStrategy) ? 0 : 1) +
                        (string.IsNullOrEmpty(viewModelPaymentDestination.DestinationAddress) ? 0 : 1) +
                        (string.IsNullOrEmpty(viewModelPaymentDestination.SelectedDestinationWalletExternalServiceId)
                            ? 0
                            : 1);
                    if (check != 1)
                    {
                        viewModel.AddModelError(
                            model => model.PaymentDestinations[index].DestinationAddress,
                            "Please choose to track either an address OR a derivation scheme OR an existing NBXplorer Wallet External Service",
                            ModelState);
                        viewModel.AddModelError(
                            model => model.PaymentDestinations[index].DerivationStrategy,
                            "Please choose to track either an address OR a derivation scheme OR an existing NBXplorer Wallet External Service",
                            ModelState);
                        viewModel.AddModelError(
                            model => model.PaymentDestinations[index].SelectedDestinationWalletExternalServiceId,
                            "Please choose to track either an address OR a derivation scheme OR an existing NBXplorer Wallet External Service",
                            ModelState);
                    }

                    if (!string.IsNullOrEmpty(viewModelPaymentDestination.SelectedDestinationWalletExternalServiceId) &&
                        !string.IsNullOrEmpty(viewModel.CryptoCode))
                    {
                        var service = services.SingleOrDefault(data =>
                            data.Id == viewModelPaymentDestination.SelectedDestinationWalletExternalServiceId);
                        if (service == null)
                        {
                            viewModel.AddModelError(
                                model => model.PaymentDestinations[index].SelectedDestinationWalletExternalServiceId,
                                "Wallet chosen is wrong... ヽ༼ ಠ益ಠ ༽ﾉ ", ModelState);
                        }
                        else if (
                            new NBXplorerWalletService(service, _nbXplorerPublicWalletProvider, _derivationSchemeParser,
                                _derivationStrategyFactoryProvider, _nbXplorerClientProvider).GetData().CryptoCode !=
                            viewModel.CryptoCode)
                        {
                            viewModel.AddModelError(
                                model => model.PaymentDestinations[index].SelectedDestinationWalletExternalServiceId,
                                "Wallet chosen not the same crypto", ModelState);
                        }
                    }

                    BitcoinAddress address = null;
                    DerivationStrategyBase derivationStrategy = null;
                    if (!string.IsNullOrEmpty(viewModelPaymentDestination.DestinationAddress) &&
                        !string.IsNullOrEmpty(viewModel.CryptoCode))
                    {
                        try
                        {
                            var factory =
                                _derivationStrategyFactoryProvider.GetDerivationStrategyFactory(viewModel.CryptoCode);
                            address = BitcoinAddress.Create(viewModelPaymentDestination.DestinationAddress,
                                factory.Network);
                        }
                        catch (Exception)
                        {
                            viewModel.AddModelError(
                                model => model.PaymentDestinations[index].DestinationAddress,
                                "Invalid Address", ModelState);
                        }
                    }

                    if (!string.IsNullOrEmpty(viewModelPaymentDestination.DerivationStrategy) &&
                        !string.IsNullOrEmpty(viewModel.CryptoCode))
                    {
                        try
                        {
                            var factory =
                                _derivationStrategyFactoryProvider.GetDerivationStrategyFactory(viewModel.CryptoCode);

                            derivationStrategy = _derivationSchemeParser.Parse(factory,
                                viewModelPaymentDestination.DerivationStrategy);
                        }
                        catch
                        {
                            viewModel.AddModelError(
                                model => model.PaymentDestinations[index].DerivationStrategy,
                                "Invalid Derivation Scheme", ModelState);
                        }
                    }
                }
            }


            if (!ModelState.IsValid)
            {
                return View(viewModel);
            }

            return await SetItUp(viewModel);
        }

        private async Task<IActionResult> SetItUp(CreatePaymentForwarderViewModel vm)
        {
            var client = _nbXplorerClientProvider.GetClient(vm.CryptoCode);
            var presetName = $"Generated_PaymentForwarder_{vm.CryptoCode}";
            var sourceExternalServiceId = vm.SelectedSourceWalletExternalServiceId;

            if (vm.GenerateSourceWallet)
            {
                var newSeed = new Mnemonic(Wordlist.English, WordCount.Twelve);
                var data = new NBXplorerWalletExternalServiceData()
                {
                    CryptoCode = vm.CryptoCode,
                    DerivationStrategy = newSeed.DeriveExtKey().ToString(client.Network.NBitcoinNetwork) + "-[p2sh]",
                    PrivateKeys = new List<PrivateKeyDetails>()
                    {
                        new PrivateKeyDetails()
                        {
                            MnemonicSeed = newSeed.ToString()
                        }
                    }
                };

                var sourceWallet = new ExternalServiceData()
                {
                    Name = presetName + "_Source_Wallet",
                    UserId = _userManager.GetUserId(User),
                    Type = NBXplorerWalletService.NBXplorerWalletServiceType,
                    DataJson = client.Serializer.ToString(data)
                };
                await _externalServiceManager.AddOrUpdateExternalServiceData(sourceWallet);
                sourceExternalServiceId = sourceWallet.Id;
            }

            var recipe = new Recipe()
            {
                Name = presetName,
                Description = "Generated from a preset",
                UserId = _userManager.GetUserId(User),
                Enabled = false
            };
            await _recipeManager.AddOrUpdateRecipe(recipe);

            var recipeTrigger = new RecipeTrigger()
            {
                ExternalServiceId = sourceExternalServiceId,
                TriggerId = NBXplorerBalanceTrigger.Id,
                RecipeId = recipe.Id,
                DataJson = client.Serializer.ToString(new NBXplorerBalanceTriggerParameters()
                {
                    BalanceValue = vm.SendEntireBalance ? 0 : vm.SendBalanceValue,
                    BalanceComparer = vm.SendEntireBalance
                        ? BalanceComparer.GreaterThan
                        : BalanceComparer.GreaterThanOrEqual,
                    BalanceMoneyUnit = vm.SendBalanceMoneyUnit
                })
            };

            await _recipeManager.AddOrUpdateRecipeTrigger(recipeTrigger);

            var recipeActionGroup = new RecipeActionGroup()
            {
                RecipeId = recipe.Id
            };
            await _recipeManager.AddRecipeActionGroup(recipeActionGroup);
            var recipeActionGroupIndex = 0;
            var ouputs = new List<SendTransactionData.TransactionOutput>();
            foreach (var paymentDestination in vm.PaymentDestinations)
            {
                var destinationExternalServiceId = paymentDestination.SelectedDestinationWalletExternalServiceId;
                NBXplorerWalletExternalServiceData data = null;

                if (string.IsNullOrEmpty(destinationExternalServiceId))
                {
                    data = new NBXplorerWalletExternalServiceData()
                    {
                        CryptoCode = vm.CryptoCode,
                        Address = paymentDestination.DestinationAddress,
                        DerivationStrategy = paymentDestination.DerivationStrategy,
                    };

                    var wallet = new ExternalServiceData()
                    {
                        Name = presetName + $"_Dest_Wallet_{data.DerivationStrategy}",
                        UserId = _userManager.GetUserId(User),
                        Type = NBXplorerWalletService.NBXplorerWalletServiceType,
                        DataJson = client.Serializer.ToString(data)
                    };
                    await _externalServiceManager.AddOrUpdateExternalServiceData(wallet);
                    destinationExternalServiceId = wallet.Id;
                }
                else
                {
                    var service = vm.Services.Items.Cast<ExternalServiceData>()
                        .Single(serviceData => serviceData.Id == destinationExternalServiceId);
                    data = new NBXplorerWalletService(service, _nbXplorerPublicWalletProvider, _derivationSchemeParser,
                        _derivationStrategyFactoryProvider, _nbXplorerClientProvider).GetData();
                }

                var recipeAction = new RecipeAction()
                {
                    RecipeId = recipe.Id,
                    RecipeActionGroupId = recipeActionGroup.Id,
                    ActionId = new GenerateNextAddressDataActionHandler().ActionId,
                    ExternalServiceId = destinationExternalServiceId,
                    Order = recipeActionGroupIndex,
                    DataJson = JsonConvert.SerializeObject(new GenerateNextAddressData())
                };
                await _recipeManager.AddOrUpdateRecipeAction(recipeAction);

                ouputs.Add(new SendTransactionData.TransactionOutput()
                {
                    DestinationAddress = "{{Action" + recipeActionGroupIndex + "}}",
                    Amount = "{{TriggerData.Balance * (" + paymentDestination.AmountPercentage + "/100)}}",
                    SubtractFeesFromOutput = paymentDestination.SubtractFeesFromOutput
                });

                recipeActionGroupIndex++;
            }

            var sweepRecipeAction = new RecipeAction()
            {
                RecipeId = recipe.Id,
                RecipeActionGroupId = recipeActionGroup.Id,
                ActionId = new SendTransactionDataActionHandler().ActionId,
                ExternalServiceId = sourceExternalServiceId,
                Order = recipeActionGroupIndex,
                DataJson = JsonConvert.SerializeObject(new SendTransactionData()
                {
                    Outputs = ouputs
                })
            };

            await _recipeManager.AddOrUpdateRecipeAction(sweepRecipeAction);

            return RedirectToAction("EditRecipe", "Recipes", new
            {
                id = recipe.Id,
                statusMessage =
                    "Preset generated. Recipe is currently disabled for now. Please verify details are correct before enabling!"
            });
        }
    }

    public class CreatePaymentForwarderViewModel
    {
        public SelectList CryptoCodes { get; set; }
        public SelectList Services { get; set; }

        [Display(Name = "Crypto")] [Required] public string CryptoCode { get; set; }

        [Display(Name = "Existing NBXplorer Wallet")]
        public string SelectedSourceWalletExternalServiceId { get; set; }

        [Display(Name = "Generate new NBXplorer Wallet for receiving funds (P2SH-Segwit)")]
        public bool GenerateSourceWallet { get; set; }

        [Display(Name = "Send whenever there is any balance")]
        public bool SendEntireBalance { get; set; }

        [Display(Name = "When balance is at least")]
        public decimal SendBalanceValue { get; set; }

        public MoneyUnit SendBalanceMoneyUnit { get; set; } = MoneyUnit.BTC;

        public List<PaymentDestination> PaymentDestinations { get; set; }
        public string Action { get; set; }

        public class PaymentDestination
        {
            [Display(Name = "Existing NBXplorer Wallet")]
            public string SelectedDestinationWalletExternalServiceId { get; set; }

            [Display(Name = "Destination Address")]
            public string DestinationAddress { get; set; }

            [Display(Name = "Derivation Strategy")]
            public string DerivationStrategy { get; set; }

            [Display(Name = "Percentage amount to forward")]
            [Required]
            [Range(0, 100)]
            public decimal AmountPercentage { get; set; }

            [Display(Name = "Subtract fees from this destination")]
            public bool SubtractFeesFromOutput { get; set; }
        }
    }
}