using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using BtcTransmuter.Abstractions.Actions;
using BtcTransmuter.Abstractions.ExternalServices;
using BtcTransmuter.Abstractions.Recipes;
using BtcTransmuter.Data.Entities;
using BtcTransmuter.Data.Models;
using BtcTransmuter.Extension.Email.ExternalServices.Smtp;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Caching.Memory;
using MimeKit;
using MimeKit.Text;

namespace BtcTransmuter.Extension.Email.Actions.SendEmail
{
    [Route("email-plugin/actions/[controller]")]
    [Authorize]
    public class SendEmailController : BaseActionController<SendEmailController.SendEmailViewModel, SendEmailData>
    {
        private readonly IExternalServiceManager _externalServiceManager;

        public SendEmailController(IMemoryCache memoryCache, UserManager<User> userManager,
            IRecipeManager recipeManager, IExternalServiceManager externalServiceManager) : base(memoryCache,
            userManager, recipeManager, externalServiceManager)
        {
            _externalServiceManager = externalServiceManager;
        }

        protected override async Task<SendEmailViewModel> BuildViewModel(RecipeAction from)
        {
            var fromData = from.Get<SendEmailData>();
            var services = await _externalServiceManager.GetExternalServicesData(new ExternalServicesDataQuery()
            {
                Type = new[] {SmtpService.SmtpExternalServiceType},
                UserId = GetUserId()
            });
            return new SendEmailViewModel
            {
                
                RecipeId = from.RecipeId,
                ExternalServiceId = from.ExternalServiceId,
                Body = fromData.Body,
                Subject = fromData.Subject,
                To = fromData.To,
                From = fromData.From,
                IsHTML = fromData.IsHTML,
                ExternalServices = new SelectList(services, nameof(ExternalServiceData.Id),
                    nameof(ExternalServiceData.Name), from.ExternalServiceId),
            };
        }

        protected async Task<string> SendTestEmail(SmtpService service, string email)
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

        protected override async Task<(RecipeAction ToSave, SendEmailViewModel showViewModel)> BuildModel(
            SendEmailViewModel viewModel, RecipeAction mainModel)
        {
            
            if(ModelState.IsValid && !string.IsNullOrEmpty(viewModel.TestEmail))
            {
                var data = new ExternalServiceData();
                data.Set(viewModel);
                var smtpService = new SmtpService(data);
                var error = await SendTestEmail(smtpService,  viewModel.TestEmail);
                if (string.IsNullOrEmpty(error))
                {
                    ModelState.AddModelError(nameof(viewModel.TestEmail), error
                    );
                }
                else
                {
                    ModelState.AddModelError(nameof(viewModel.TestEmail), "Email sent successfully, confirm that you received it");
                }
               
            }else if(ModelState.IsValid)
            {
                mainModel.ExternalServiceId = viewModel.ExternalServiceId;
                mainModel.Set<SendEmailData>(viewModel);
                return (mainModel, null);
            }

            var services = await _externalServiceManager.GetExternalServicesData(new ExternalServicesDataQuery()
            {
                Type = new[] {SmtpService.SmtpExternalServiceType},
                UserId = GetUserId()
            });


            viewModel.ExternalServices = new SelectList(services, nameof(ExternalServiceData.Id),
                nameof(ExternalServiceData.Name), viewModel.ExternalServiceId);
            return (null, viewModel);
        }

        public class SendEmailViewModel : SendEmailData, IUseExternalService, IActionViewModel
        {
            public string RecipeId { get; set; }
            public string RecipeActionIdInGroupBeforeThisOne { get; set; }
            public SelectList ExternalServices { get; set; }
            [Display(Name = "SMTP External Service")]
            [Required] public string ExternalServiceId { get; set; }

            [EmailAddress]
            [Display(Name = "Send test email from and to this address")]
            public string TestEmail { get; set; }
        }
    }
}