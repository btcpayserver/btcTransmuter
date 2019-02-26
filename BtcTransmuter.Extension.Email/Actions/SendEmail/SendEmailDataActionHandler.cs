using System.Collections.Generic;
using System.Threading.Tasks;
using BtcTransmuter.Abstractions.Actions;
using BtcTransmuter.Data.Entities;
using BtcTransmuter.Extension.Email.ExternalServices.Smtp;
using MimeKit;
using MimeKit.Text;

namespace BtcTransmuter.Extension.Email.Actions.SendEmail
{
    public class SendEmailDataActionHandler : BaseActionHandler<SendEmailData>
    {
        public override string ActionId => "SendEmail";
        public override string Name => "Send Email";

        public override string Description =>
            "Send an email using an smtp external service";

        public override string ViewPartial => "ViewSendEmailAction";
        
        public override string ControllerName => "SendEmail";

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