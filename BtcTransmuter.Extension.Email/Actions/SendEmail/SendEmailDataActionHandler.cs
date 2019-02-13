using System;
using System.Collections.Generic;
using System.Net.Mail;
using System.Threading.Tasks;
using BtcTransmuter.Abstractions.Actions;
using BtcTransmuter.Abstractions.Models;
using BtcTransmuter.Data;
using BtcTransmuter.Data.Entities;
using BtcTransmuter.Extension.Email.ExternalServices;
using Newtonsoft.Json.Linq;

namespace BtcTransmuter.Extension.Email.Actions.SendEmail
{
    public class SendEmailDataActionHandler : BaseActionHandler<SendEmailData>
    {
        private readonly SendEmailActionDescriptor _sendEmailActionDescriptor;

        public SendEmailDataActionHandler(SendEmailActionDescriptor sendEmailActionDescriptor)
        {
            _sendEmailActionDescriptor = sendEmailActionDescriptor;
        }

        public override string ActionId => _sendEmailActionDescriptor.ActionId;

        public override ValidationResult Validate(string data)
        {
            var errors = new Dictionary<string, string>();
            try
            {
                var parsed = JObject.Parse(data).ToObject<SendEmailData>();

                if (string.IsNullOrEmpty(parsed.To))
                {
                    errors.Add(nameof(SendEmailData.To), "To is required");
                }
                if (string.IsNullOrEmpty(parsed.From))
                {
                    errors.Add(nameof(SendEmailData.From), "From is required");
                }
            }
            catch (Exception e)
            {
                errors.Add(string.Empty, "Could not parse data");
            }
            
            return new ValidationResult()
            {
                Errors = errors
            };
        }

        protected override Task<bool> CanExecute(object triggerData, RecipeAction recipeAction)
        {
            return Task.FromResult(recipeAction.ActionId == ActionId);
        }

        protected override async Task<ActionHandlerResult> Execute(object triggerData, RecipeAction recipeAction,
            SendEmailData actionData)
        {
            var smtpService = new SmtpService(recipeAction.ExternalService);

            var message = new MailMessage(
                InterpolateString(actionData.From, triggerData),
                InterpolateString(actionData.To, triggerData),
                InterpolateString(actionData.Subject, triggerData),
                InterpolateString(actionData.Body, triggerData));

            await smtpService.SendEmail(message);
            return new ActionHandlerResult()
            {
                Result = $"Sent email to {message.To} from {message.From} with subject {message.Subject} and body {message.Body}"
            };
        }
    }
}