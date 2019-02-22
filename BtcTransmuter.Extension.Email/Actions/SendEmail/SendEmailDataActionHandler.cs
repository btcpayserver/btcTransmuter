using System;
using System.Collections.Generic;
using System.Net.Mail;
using System.Threading.Tasks;
using BtcTransmuter.Abstractions.Actions;
using BtcTransmuter.Abstractions.Helpers;
using BtcTransmuter.Data.Entities;
using BtcTransmuter.Extension.Email.ExternalServices;
using BtcTransmuter.Extension.Email.ExternalServices.Smtp;
using BtcTransmuter.Extension.Email.Triggers.ReceivedEmail;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using MimeKit;
using MimeKit.Text;

namespace BtcTransmuter.Extension.Email.Actions.SendEmail
{
    public class SendEmailDataActionHandler : BaseActionHandler<SendEmailData>, IActionDescriptor
    {
        public override string ActionId => "SendEmail";
        public string Name => "Send Email";

        public string Description =>
            "Send an email using an smtp external service";

        public string ViewPartial => "ViewSendEmailAction";

        public Task<IActionResult> EditData(RecipeAction data)
        {
            using (var scope = DependencyHelper.ServiceScopeFactory.CreateScope())
            {
                var identifier = $"{Guid.NewGuid()}";
                var memoryCache = scope.ServiceProvider.GetService<IMemoryCache>();
                memoryCache.Set(identifier, data, new MemoryCacheEntryOptions()
                {
                    SlidingExpiration = TimeSpan.FromMinutes(60)
                });

                return Task.FromResult<IActionResult>(new RedirectToActionResult(
                    nameof(SendEmailController.EditData),
                    "SendEmail", new
                    {
                        identifier
                    }));
            }
        }

        protected override Task<bool> CanExecute(object triggerData, RecipeAction recipeAction)
        {
            return Task.FromResult(recipeAction.ActionId == ActionId);
        }

        protected override async Task<ActionHandlerResult> Execute(object triggerData, RecipeAction recipeAction,
            SendEmailData actionData)
        {
            var smtpService = new SmtpService(recipeAction.ExternalService);

            var message = new MimeMessage(new List<InternetAddress>()
                {
                    InternetAddress.Parse(InterpolateString(actionData.From, triggerData))
                }, new List<InternetAddress>()
                {
                    InternetAddress.Parse(InterpolateString(actionData.To, triggerData))
                }, InterpolateString(actionData.Subject, triggerData),
                new TextPart(TextFormat.Plain)
                {
                    Text = InterpolateString(actionData.Body, triggerData)
                });

            await smtpService.SendEmail(message);
            return new ActionHandlerResult()
            {
                Result =
                    $"Sent email to {message.To} from {message.From} with subject {message.Subject} and body {message.Body}"
            };
        }
    }
}