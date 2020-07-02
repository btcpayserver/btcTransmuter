using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using BtcTransmuter.Abstractions.ExternalServices;
using BtcTransmuter.Data.Entities;
using BtcTransmuter.Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using MimeKit;
using MimeKit.Text;

namespace BtcTransmuter.Extension.Email.ExternalServices.Smtp
{
    [Route("email-plugin/external-services/smtp")]
    [Authorize]
    public class SmtpController : BaseExternalServiceController<EditSmtpExternalServiceViewModel>
    {
        public SmtpController(IExternalServiceManager externalServiceManager, UserManager<User> userManager,
            IMemoryCache memoryCache) : base(externalServiceManager, userManager, memoryCache)
        {
        }

        protected override string ExternalServiceType => SmtpService.SmtpExternalServiceType;

        protected override Task<EditSmtpExternalServiceViewModel> BuildViewModel(ExternalServiceData data)
        {
            var serviceData = new SmtpService(data).GetData();
            return Task.FromResult(new EditSmtpExternalServiceViewModel()
            {
                Password = serviceData.Password,
                Port = serviceData.Port,
                Server = serviceData.Server,
                Username = serviceData.Username,
                SSL = serviceData.SSL
            });
        }

        protected override async Task<(ExternalServiceData ToSave, EditSmtpExternalServiceViewModel showViewModel)>
            BuildModel(
                EditSmtpExternalServiceViewModel viewModel, ExternalServiceData mainModel)
        {
            if (ModelState.IsValid && !string.IsNullOrEmpty(viewModel.TestEmail))
            {
                var data = new ExternalServiceData();
                data.Set(viewModel);
                data.Type = ExternalServiceType;
                var smtpService = new SmtpService(data);
                var error = await SendTestEmail(smtpService, viewModel.TestEmail);
                if (string.IsNullOrEmpty(error))
                {
                    viewModel.TestEmail = string.Empty;
                }
                else
                {
                    ModelState.AddModelError(nameof(viewModel.TestEmail),error);
                }
            }

            if (!ModelState.IsValid)
            {
                return (null, viewModel);
            }

            mainModel.Set(viewModel);
            return (mainModel, null);
        }


        private async Task<string> SendTestEmail(SmtpService service, string email)
        {
            try
            {
                await service.SendEmail(new MimeMessage(new List<InternetAddress>()
                {
                    InternetAddress.Parse(email)
                }, new List<InternetAddress>()
                {
                    InternetAddress.Parse(email)
                }, "BTCTransmuter Test Email", new TextPart(TextFormat.Plain)
                {
                    Text = "Just testing your email setup for BTC Transmuter (ﾉ◕ヮ◕)ﾉ*:・ﾟ✧"
                }));
                return null;
            }
            catch (Exception e)
            {
                return e.Message;
            }
        }
    }

    public class EditSmtpExternalServiceViewModel : SmtpExternalServiceData
    {
        [EmailAddress]
        [Display(Name = "Send test email from and to this address to check if your settings are valid")]
        public string TestEmail { get; set; }
    }
}
