using System.Net.Mail;
using System.Threading.Tasks;
using BtcTransmuter.Abstractions.Actions;
using BtcTransmuter.Data;
using BtcTransmuter.Data.Entities;
using BtcTransmuter.Extension.Email.ExternalServices;

namespace BtcTransmuter.Extension.Email.Actions.SendEmail
{
    public class SendEmailDataActionHandler : BaseActionHandler<SendEmailData>
    {
        private readonly SendEmailActionDescriptor _sendEmailActionDescriptor;

        public SendEmailDataActionHandler(SendEmailActionDescriptor sendEmailActionDescriptor)
        {
            _sendEmailActionDescriptor = sendEmailActionDescriptor;
        }

        protected override Task<bool> CanExecute(object triggerData, RecipeAction recipeAction)
        {
            return Task.FromResult(recipeAction.ActionId == _sendEmailActionDescriptor.ActionId);
        }

        protected override async Task<bool> Execute(object triggerData, RecipeAction recipeAction,
            SendEmailData actionData)
        {
            var smtpService = new SmtpService(recipeAction.ExternalService);

            await smtpService.SendEmail(
                new MailMessage(
                    InterpolateString(actionData.From, triggerData),
                    InterpolateString(actionData.To, triggerData),
                    InterpolateString(actionData.Subject, triggerData),
                    InterpolateString(actionData.Body, triggerData)));
            return true;
        }
    }
}