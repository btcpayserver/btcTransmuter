using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BtcTransmuter.Abstractions.ExternalServices;
using BtcTransmuter.Data.Entities;
using BtcTransmuter.Data.Models;
using ExchangeSharp;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Caching.Memory;

namespace BtcTransmuter.Extension.Exchange.ExternalServices.Exchange
{
    [Route("exchange-plugin/external-services/exchange")]
    [Authorize]
    public class
        ExchangeController : BaseExternalServiceController<ExchangeController.EditExchangeExternalServiceDataViewModel>
    {
        public ExchangeController(IExternalServiceManager externalServiceManager, UserManager<User> userManager,
            IMemoryCache memoryCache) : base(externalServiceManager, userManager, memoryCache)
        {
        }

        protected override string ExternalServiceType => ExchangeService.ExchangeServiceType;

        protected override Task<EditExchangeExternalServiceDataViewModel> BuildViewModel(ExternalServiceData data)
        {
            return Task.FromResult(new EditExchangeExternalServiceDataViewModel(new ExchangeService(data).GetData(),
                ExchangeService.GetAvailableExchanges()));
        }

        protected override async
            Task<(ExternalServiceData ToSave, EditExchangeExternalServiceDataViewModel showViewModel)>
            BuildModel(EditExchangeExternalServiceDataViewModel viewModel, ExternalServiceData mainModel)
        {
            //current External Service data
            var externalServiceData = mainModel;

            if (!ModelState.IsValid)
            {
                return (null,
                    new EditExchangeExternalServiceDataViewModel(viewModel, ExchangeService.GetAvailableExchanges()));
            }

            //current External Service data
            externalServiceData.Set((ExchangeExternalServiceData) viewModel);
            var exchangeService = new ExchangeService(externalServiceData);

            if (!await exchangeService.TestAccess())
            {
                ModelState.AddModelError(String.Empty,
                    "Could not connect with current settings. Transmuter tests against fetching your balance amount from the exchange so you would need to enable that option if available");

                return (null,
                    new EditExchangeExternalServiceDataViewModel(viewModel, ExchangeService.GetAvailableExchanges()));
            }

            return (externalServiceData, null);
        }


        public class EditExchangeExternalServiceDataViewModel : ExchangeExternalServiceData
        {
            public SelectList Exchanges { get; set; }

            public EditExchangeExternalServiceDataViewModel(ExchangeExternalServiceData serviceData,
                IEnumerable<IExchangeAPI> exchangeApis)
            {
                OverrideUrl = serviceData.OverrideUrl;
                ExchangeName = serviceData.ExchangeName;
                PublicKey = serviceData.PublicKey;
                PassPhrase = serviceData.PassPhrase;
                PrivateKey = serviceData.PrivateKey;
                LastCheck = serviceData.LastCheck;
                PairedDate = serviceData.PairedDate;
                Exchanges = new SelectList(exchangeApis, nameof(IExchangeAPI.Name), nameof(IExchangeAPI.Name),
                    ExchangeName);
            }

            public EditExchangeExternalServiceDataViewModel()
            {
            }
        }
    }
}