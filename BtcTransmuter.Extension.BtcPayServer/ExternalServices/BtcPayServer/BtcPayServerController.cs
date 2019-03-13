using System;
using System.Threading.Tasks;
using BtcTransmuter.Abstractions.ExternalServices;
using BtcTransmuter.Data.Entities;
using BtcTransmuter.Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using NBitcoin;
using NBitpayClient;

namespace BtcTransmuter.Extension.BtcPayServer.ExternalServices.BtcPayServer
{
    [Route("btcpayserver-plugin/external-services/btcpayserver")]
    [Authorize]
    public class BtcPayServerController : BaseExternalServiceController<EditBtcPayServerDataViewModel>
    {
        public BtcPayServerController(IExternalServiceManager externalServiceManager, UserManager<User> userManager,
            IMemoryCache memoryCache) : base(externalServiceManager, userManager, memoryCache)
        {
        }

        protected override string ExternalServiceType => BtcPayServerService.BtcPayServerServiceType;

        protected override async Task<EditBtcPayServerDataViewModel> BuildViewModel(ExternalServiceData data)
        {
            var client = new BtcPayServerService(data);
            var clientData = client.GetData();
            return new EditBtcPayServerDataViewModel()
            {
                Seed = clientData.Seed ?? new Mnemonic(Wordlist.English, WordCount.Twelve).ToString(),
                Server = clientData.Server,
                PairingUrl = await client.GetPairingUrl(data.Name),
                Paired = await client.CheckAccess()
            };
        }

        protected override async Task<(ExternalServiceData ToSave, EditBtcPayServerDataViewModel showViewModel)>
            BuildModel(EditBtcPayServerDataViewModel viewModel, ExternalServiceData mainModel)
        {
            if (viewModel.Action == "unpair")
            {
                viewModel.Seed = null;
            }

            //current External Service data
            var oldData = mainModel.Get<BtcPayServerExternalServiceData>();


            if (oldData.Seed == viewModel.Seed && oldData.Server == viewModel.Server)
            {
                viewModel.LastCheck = oldData.LastCheck;
                viewModel.MonitoredInvoiceStatuses = oldData.MonitoredInvoiceStatuses;
                viewModel.PairedDate = oldData.PairedDate;
            }
            else
            {
                viewModel.PairedDate = DateTime.Now;
            }

            mainModel.Set((BtcPayServerExternalServiceData) viewModel);
            var service = new BtcPayServerService(mainModel);

            if (!ModelState.IsValid)
            {
                var serviceData = service.GetData();
                return (null, new EditBtcPayServerDataViewModel()
                {
                    Seed = serviceData.Seed ?? new Mnemonic(Wordlist.English, WordCount.Twelve).ToString(),
                    Server = serviceData.Server,
                    PairingUrl = await service.GetPairingUrl(mainModel.Name),
                    Paired = await service.CheckAccess()
                });
            }


            if (!await service.CheckAccess())
            {
                viewModel.Seed = viewModel.Seed ?? new Mnemonic(Wordlist.English, WordCount.Twelve).ToString();
                service.SetData(viewModel);
                viewModel.PairingUrl = await service.GetPairingUrl(mainModel.Name);
                viewModel.Paired = false;
                if (!string.IsNullOrEmpty(viewModel.PairingCode))
                {
                    var client = service.ConstructClient();
                    await client.AuthorizeClient(new PairingCode(viewModel.PairingCode));
                    if (!await service.CheckAccess())
                    {
                        ModelState.AddModelError(string.Empty, "Could not pair with pairing code");
                        return (null, viewModel);
                    }
                }

                ModelState.AddModelError(string.Empty, "Cannot proceed until paired");
                return (null, viewModel);
            }

            return (mainModel, null);
        }
    }
}