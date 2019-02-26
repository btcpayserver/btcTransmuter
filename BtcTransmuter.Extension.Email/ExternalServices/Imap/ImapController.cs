using System;
using System.Threading.Tasks;
using BtcTransmuter.Abstractions.ExternalServices;
using BtcTransmuter.Data.Entities;
using BtcTransmuter.Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace BtcTransmuter.Extension.Email.ExternalServices.Imap
{
    [Route("email-plugin/external-services/imap")]
    [Authorize]
    public class ImapController : BaseExternalServiceController<ImapExternalServiceData>
    {
        public ImapController(IExternalServiceManager externalServiceManager, UserManager<User> userManager, IMemoryCache memoryCache) : base(externalServiceManager, userManager, memoryCache)
        {
        }

        protected override string ExternalServiceType => ImapService.ImapExternalServiceType;
        protected override Task<ImapExternalServiceData> BuildViewModel(ExternalServiceData data)
        { 
            return Task.FromResult(new ImapService(data).GetData());
        }

        protected override async Task<(ExternalServiceData ToSave, ImapExternalServiceData showViewModel)> BuildModel(ImapExternalServiceData viewModel, ExternalServiceData mainModel)
        {
            if (!ModelState.IsValid)
            {
                return (null, viewModel);
            }
            viewModel.PairedDate = DateTime.Now;
            mainModel.Set(viewModel);

            var imapService = new ImapService(mainModel);
            var testConnection = await imapService.CreateClientAndConnect();
            if (testConnection == null)
            { 
                ModelState.AddModelError(string.Empty, "Could not connect successfully");

                return (null, viewModel);
            }

            testConnection.Dispose();
            return (mainModel, null);
        }
    }
}