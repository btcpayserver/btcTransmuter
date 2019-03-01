using System.Threading.Tasks;
using BtcTransmuter.Abstractions.ExternalServices;
using BtcTransmuter.Data.Entities;
using BtcTransmuter.Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace BtcTransmuter.Extension.Email.ExternalServices.Smtp
{
    [Route("email-plugin/external-services/smtp")]
    [Authorize]
    public class SmtpController : BaseExternalServiceController<SmtpExternalServiceData>
    {
        public SmtpController(IExternalServiceManager externalServiceManager, UserManager<User> userManager,
            IMemoryCache memoryCache) : base(externalServiceManager, userManager, memoryCache)
        {
        }

        protected override string ExternalServiceType => SmtpService.SmtpExternalServiceType;

        protected override Task<SmtpExternalServiceData> BuildViewModel(ExternalServiceData data)
        {
            return Task.FromResult(new SmtpService(data).GetData());
        }

        protected override Task<(ExternalServiceData ToSave, SmtpExternalServiceData showViewModel)> BuildModel(
            SmtpExternalServiceData viewModel, ExternalServiceData mainModel)
        {
            if (!ModelState.IsValid)
            {
                return Task.FromResult<(ExternalServiceData ToSave, SmtpExternalServiceData showViewModel)>((null,
                    viewModel));
            }

            mainModel.Set(viewModel);
            return Task.FromResult<(ExternalServiceData ToSave, SmtpExternalServiceData showViewModel)>((mainModel,
                null));
        }
    }
}