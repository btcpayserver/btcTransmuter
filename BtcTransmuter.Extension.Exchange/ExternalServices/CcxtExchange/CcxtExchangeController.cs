using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BtcTransmuter.Abstractions.ExternalServices;
using BtcTransmuter.Data.Entities;
using BtcTransmuter.Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Caching.Memory;

namespace BtcTransmuter.Extension.Exchange.ExternalServices.CcxtExchange
{
    [Route("exchange-plugin/external-services/ccxtexchange")]
    [Authorize]
    public class
        CcxtExchangeController : BaseExternalServiceController<CcxtExchangeController.EditExchangeExternalServiceDataViewModel>
    {
        public CcxtExchangeController(IExternalServiceManager externalServiceManager, UserManager<User> userManager,
            IMemoryCache memoryCache) : base(externalServiceManager, userManager, memoryCache)
        {
        }

        protected override string ExternalServiceType => CcxtExchangeService.ExchangeServiceType;

        protected override Task<EditExchangeExternalServiceDataViewModel> BuildViewModel(ExternalServiceData data)
        {
            return Task.FromResult(new EditExchangeExternalServiceDataViewModel(new CcxtExchangeService(data).GetData(),
                CcxtExchangeService.GetAvailableExchanges()));
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
                    new EditExchangeExternalServiceDataViewModel(viewModel, CcxtExchangeService.GetAvailableExchanges()));
            }

            //current External Service data
            externalServiceData.Set((ExchangeExternalServiceData) viewModel);
            var exchangeService = new CcxtExchangeService(externalServiceData);

            if (!await exchangeService.TestAccess())
            {
                ModelState.AddModelError(String.Empty, "Could not connect with current settings");


                return (null,
                    new EditExchangeExternalServiceDataViewModel(viewModel, CcxtExchangeService.GetAvailableExchanges()));
            }

            return (externalServiceData, null);
        }


        public class EditExchangeExternalServiceDataViewModel : ExchangeExternalServiceData
        {
            public SelectList Exchanges { get; set; }

            public EditExchangeExternalServiceDataViewModel(ExchangeExternalServiceData serviceData,
                IEnumerable<string> exchangeApis)
            {
                OverrideUrl = serviceData.OverrideUrl;
                ExchangeName = serviceData.ExchangeName;
                PublicKey = serviceData.PublicKey;
                PassPhrase = serviceData.PassPhrase;
                PrivateKey = serviceData.PrivateKey;
                LastCheck = serviceData.LastCheck;
                PairedDate = serviceData.PairedDate;
                Exchanges = new SelectList(exchangeApis,
                    ExchangeName);
            }

            public EditExchangeExternalServiceDataViewModel()
            {
            }
        }
    }
}