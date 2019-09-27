using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BtcTransmuter.Abstractions.ExternalServices;
using BtcTransmuter.Data.Entities;
using BtcTransmuter.Data.Models;
using BtcTransmuter.Extension.NBXplorer.Models;
using BtcTransmuter.Extension.NBXplorer.Services;
using BTCPayServer.Lightning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Caching.Memory;

namespace BtcTransmuter.Extension.Lightning.ExternalServices.LightningNode
{
    [Route("lightning-plugin/external-services/lightning-node")]
    [Authorize]
    public class
        LightningNodeController : BaseExternalServiceController<
            LightningNodeController.EditLightningNodeExternalServiceDataViewModel>
    {
        private readonly NBXplorerOptions _nbXplorerOptions;
        private readonly NBXplorerClientProvider _nbXplorerClientProvider;
        private readonly NBXplorerSummaryProvider _nbXplorerSummaryProvider;
        private readonly SocketFactory _socketFactory;

        public LightningNodeController(IExternalServiceManager externalServiceManager, UserManager<User> userManager,
            IMemoryCache memoryCache, NBXplorerOptions nbXplorerOptions,
            NBXplorerClientProvider nbXplorerClientProvider, NBXplorerSummaryProvider nbXplorerSummaryProvider,
            SocketFactory socketFactory) : base(externalServiceManager, userManager, memoryCache)
        {
            _nbXplorerOptions = nbXplorerOptions;
            _nbXplorerClientProvider = nbXplorerClientProvider;
            _nbXplorerSummaryProvider = nbXplorerSummaryProvider;
            _socketFactory = socketFactory;
        }

        protected override string ExternalServiceType => LightningNodeService.LightningNodeServiceType;

        protected override Task<EditLightningNodeExternalServiceDataViewModel> BuildViewModel(ExternalServiceData data)
        {
            var vm = new EditLightningNodeExternalServiceDataViewModel(
                new LightningNodeService(data, _nbXplorerClientProvider, _nbXplorerSummaryProvider, _socketFactory)
                    .GetData());
            
            vm.CryptoCodes = new SelectList(_nbXplorerOptions.Cryptos?.ToList() ?? new List<string>(),
                vm.CryptoCode);
            return Task.FromResult(vm);
        }

        protected override async
            Task<(ExternalServiceData ToSave, EditLightningNodeExternalServiceDataViewModel showViewModel)>
            BuildModel(EditLightningNodeExternalServiceDataViewModel viewModel, ExternalServiceData mainModel)
        {
            var failureViewModel = new EditLightningNodeExternalServiceDataViewModel(viewModel);
            failureViewModel.CryptoCodes = new SelectList(_nbXplorerOptions.Cryptos?.ToList() ?? new List<string>(),
                viewModel.CryptoCode);
            //current External Service data
            var externalServiceData = mainModel;

            if (!ModelState.IsValid)
            {
                return (null, failureViewModel);
            }

            //current External Service data
            externalServiceData.Set((LightningNodeExternalServiceData) viewModel);
            var lightningNodeService = new LightningNodeService(externalServiceData, _nbXplorerClientProvider,
                _nbXplorerSummaryProvider, _socketFactory);


            if (!string.IsNullOrEmpty(viewModel.ConnectionString))
            {
                if (!LightningConnectionString.TryParse(viewModel.ConnectionString, false, out var connectionString,
                    out var error))
                {
                    ModelState.AddModelError(nameof(viewModel.ConnectionString), $"Invalid URL ({error})");
                    return (null, failureViewModel);
                }

                if (connectionString.ConnectionType == LightningConnectionType.LndGRPC)
                {
                    ModelState.AddModelError(nameof(viewModel.ConnectionString),
                        $"BTCPay does not support gRPC connections");
                    return (null, failureViewModel);
                }

                var canUseInternalLightning = await IsAdmin();

                bool isInternalNode = connectionString.ConnectionType == LightningConnectionType.CLightning ||
                                      connectionString.BaseUri.DnsSafeHost.StartsWith("lnd_") ||
                                      (connectionString.BaseUri.DnsSafeHost.Equals("127.0.0.1",
                                           StringComparison.InvariantCultureIgnoreCase) ||
                                       connectionString.BaseUri.DnsSafeHost.Equals("localhost",
                                           StringComparison.InvariantCultureIgnoreCase));

                if (connectionString.BaseUri.Scheme == "http")
                {
                    if (!isInternalNode)
                    {
                        ModelState.AddModelError(nameof(viewModel.ConnectionString), "The url must be HTTPS");
                        return (null, failureViewModel);
                    }
                }

                if (connectionString.MacaroonFilePath != null)
                {
                    if (!canUseInternalLightning)
                    {
                        ModelState.AddModelError(nameof(viewModel.ConnectionString),
                            "You are not authorized to use macaroonfilepath");
                        return (null, failureViewModel);
                    }

                    if (!System.IO.File.Exists(connectionString.MacaroonFilePath))
                    {
                        ModelState.AddModelError(nameof(viewModel.ConnectionString),
                            "The macaroonfilepath file does not exist");
                        return (null, failureViewModel);
                    }

                    if (!System.IO.Path.IsPathRooted(connectionString.MacaroonFilePath))
                    {
                        ModelState.AddModelError(nameof(viewModel.ConnectionString),
                            "The macaroonfilepath should be fully rooted");
                        return (null, failureViewModel);
                    }
                }

                if (isInternalNode && !canUseInternalLightning)
                {
                    ModelState.AddModelError(nameof(viewModel.ConnectionString), "Unauthorized url");
                    return (null, failureViewModel);
                }
            }

            if (!await lightningNodeService.TestAccess(Request.IsOnion()))
            {
                ModelState.AddModelError(String.Empty, "Could not connect with current settings");


                return (null, failureViewModel);
            }

            return (externalServiceData, null);
        }

        public class EditLightningNodeExternalServiceDataViewModel : LightningNodeExternalServiceData
        {
            public EditLightningNodeExternalServiceDataViewModel(LightningNodeExternalServiceData serviceData)
            {
                ConnectionString = serviceData.ConnectionString;
                CryptoCode = serviceData.CryptoCode;
            }

            public EditLightningNodeExternalServiceDataViewModel()
            {
            }

            public SelectList CryptoCodes { get; set; }
        }
    }
}