using System.Threading.Tasks;
using BtcTransmuter.Abstractions.ExternalServices;
using BtcTransmuter.Data.Entities;
using BtcTransmuter.Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace BtcTransmuter.Extension.Email.ExternalServices.Pop3
{
    [Route("email-plugin/external-services/pop3")]
    [Authorize]
    public class Pop3Controller : BaseExternalServiceController<Pop3ExternalServiceData>
    {
        public Pop3Controller(IExternalServiceManager externalServiceManager, UserManager<User> userManager, IMemoryCache memoryCache) : base(externalServiceManager, userManager, memoryCache)
        {
        }

        protected override string ExternalServiceType => Pop3Service.Pop3ExternalServiceType;
        protected override Task<Pop3ExternalServiceData> BuildViewModel(ExternalServiceData data)
        { 
            return Task.FromResult(new Pop3Service(data).GetData());
        }

        protected override async Task<(ExternalServiceData ToSave, Pop3ExternalServiceData showViewModel)> BuildModel(Pop3ExternalServiceData viewModel, ExternalServiceData mainModel)
        {
            if (!ModelState.IsValid)
            {
                return (null, viewModel);
            }
            mainModel.Set(viewModel);

            var imapService = new Pop3Service(mainModel);
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