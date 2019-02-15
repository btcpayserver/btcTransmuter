using System.Net.Mail;
using System.Threading.Tasks;
using BtcTransmuter.Abstractions.Actions;
using BtcTransmuter.Data.Entities;
using BtcTransmuter.Extension.Email.ExternalServices;

namespace BtcTransmuter.Extension.Email.Actions.SendEmail
{
    public class SendEmailDataActionHandler : BaseActionHandler<SendEmailData>, IActionDescriptor
    {
        public override string ActionId => "SendEmail";
        string IActionDescriptor.Name => "Send Email";

        string IActionDescriptor.Description =>
            "Send an email using an smtp external service";

        public string ViewPartial => "ViewSendEmailAction";

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
                Result =
                    $"Sent email to {message.To} from {message.From} with subject {message.Subject} and body {message.Body}"
            };
        }
    }
}