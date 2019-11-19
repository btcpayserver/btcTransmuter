using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BtcTransmuter.Abstractions.ExternalServices;
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

namespace BtcTransmuter.Extension.Lightning.ExternalServices.NBXplorerWallet
{
    [Route("nbxplorer-plugin/external-services/NBXplorerWallet")]
    [Authorize]
    public class
        NBXplorerWalletController : BaseExternalServiceController<
            NBXplorerWalletController.EditNBXplorerWalletExternalServiceDataViewModel>
    {
        private readonly NBXplorerOptions _nbXplorerOptions;
        private readonly NBXplorerPublicWalletProvider _nbXplorerPublicWalletProvider;
        private readonly DerivationSchemeParser _derivationSchemeParser;
        private readonly NBXplorerClientProvider _nbXplorerClientProvider;

        public NBXplorerWalletController(IExternalServiceManager externalServiceManager, UserManager<User> userManager,
            IMemoryCache memoryCache, NBXplorerOptions nbXplorerOptions,
            NBXplorerPublicWalletProvider nbXplorerPublicWalletProvider,
            DerivationSchemeParser derivationSchemeParser,
            NBXplorerClientProvider nbXplorerClientProvider) : base(
            externalServiceManager, userManager, memoryCache)
        {
            _nbXplorerOptions = nbXplorerOptions;
            _nbXplorerPublicWalletProvider = nbXplorerPublicWalletProvider;
            _derivationSchemeParser = derivationSchemeParser;
            _nbXplorerClientProvider = nbXplorerClientProvider;
        }

        protected override string ExternalServiceType => NBXplorerWalletService.NBXplorerWalletServiceType;

        protected override Task<EditNBXplorerWalletExternalServiceDataViewModel> BuildViewModel(
            ExternalServiceData data)
        {
            var vm = new EditNBXplorerWalletExternalServiceDataViewModel(
                new NBXplorerWalletService(data, _nbXplorerPublicWalletProvider, _derivationSchemeParser, _nbXplorerClientProvider)
                    .GetData());

            vm.CryptoCodes = new SelectList(_nbXplorerOptions.Cryptos?.ToList() ?? new List<string>(),
                vm.CryptoCode);
            return Task.FromResult(vm);
        }

        protected override async
            Task<(ExternalServiceData ToSave, EditNBXplorerWalletExternalServiceDataViewModel showViewModel)>
            BuildModel(EditNBXplorerWalletExternalServiceDataViewModel viewModel, ExternalServiceData mainModel)
        {
            var failureViewModel = new EditNBXplorerWalletExternalServiceDataViewModel(viewModel);
            failureViewModel.CryptoCodes = new SelectList(_nbXplorerOptions.Cryptos?.ToList() ?? new List<string>(),
                viewModel.CryptoCode);

            if (viewModel.Action.Equals("add-private-key", StringComparison.InvariantCultureIgnoreCase))
            {
                failureViewModel.PrivateKeys.Add(new PrivateKeyDetails());
                return (null, failureViewModel);
            }

            if (viewModel.Action.StartsWith("remove-private-key", StringComparison.InvariantCultureIgnoreCase))
            {
                var index = int.Parse(viewModel.Action.Substring(viewModel.Action.IndexOf(":") + 1));
                failureViewModel.PrivateKeys.RemoveAt(index);
                return (null, failureViewModel);
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
                    var factory = client.Network.DerivationStrategyFactory;
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
                    var factory = client.Network.DerivationStrategyFactory;

                    derivationStrategy = _derivationSchemeParser.Parse(factory, viewModel.DerivationStrategy);
                }
                catch
                {
                    ModelState.AddModelError(nameof(viewModel.DerivationStrategy), "Invalid Derivation Scheme");
                }
            }

            //current External Service data
            var externalServiceData = mainModel;

            externalServiceData.Set( viewModel);
            if (!ModelState.IsValid)
            {
                return (null, failureViewModel);
            }

            if (derivationStrategy != null)
            {
                client.Track(TrackedSource.Create(derivationStrategy));
            }

            if (address != null)
            {
                client.Track(TrackedSource.Create(address));
            }

            return (externalServiceData, null);
        }

        public class EditNBXplorerWalletExternalServiceDataViewModel : NBXplorerWalletExternalServiceData
        {
            public EditNBXplorerWalletExternalServiceDataViewModel(NBXplorerWalletExternalServiceData serviceData)
            {
                CryptoCode = serviceData.CryptoCode;
                DerivationStrategy = serviceData.DerivationStrategy;
                Address = serviceData.Address;
                PrivateKeys = serviceData.PrivateKeys;
            }

            public EditNBXplorerWalletExternalServiceDataViewModel()
            {
            }

            public SelectList CryptoCodes { get; set; }
            public string Action { get; set; }
        }
    }
}