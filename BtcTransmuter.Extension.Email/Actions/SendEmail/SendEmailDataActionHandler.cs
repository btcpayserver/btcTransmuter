using System.Collections.Generic;
using System.Threading.Tasks;
using BtcTransmuter.Abstractions.Actions;
using BtcTransmuter.Data.Entities;
using BtcTransmuter.Extension.DynamicServices;
using BtcTransmuter.Extension.Email.ExternalServices.Smtp;
using MimeKit;
using MimeKit.Text;

namespace BtcTransmuter.Extension.Email.Actions.SendEmail
{
    public class SendEmailDataActionHandler : BaseActionHandler<SendEmailData, string>
    {
        public override string ActionId => "SendEmail";
        public override string Name => "Send Email";

        public override string Description =>
            "Send an email using an smtp external service";

        public override string ViewPartial => "ViewSendEmailAction";
        
        public override string ControllerName => "SendEmail";

        protected override async Task<TypedActionHandlerResult<string>> Execute(Dictionary<string, object> data, RecipeAction recipeAction,
            SendEmailData actionData)
        {
            var externalService = await recipeAction.GetExternalService();
            var smtpService = new SmtpService(externalService);

            var message = new MimeMessage(new List<InternetAddress>()
                {
                    InternetAddress.Parse(InterpolateString(actionData.From, data))
                }, new List<InternetAddress>()
                {
                    InternetAddress.Parse(InterpolateString(actionData.To, data))
                }, InterpolateString(actionData.Subject, data),
                new TextPart(actionData.IsHTML ? TextFormat.Html : TextFormat.Plain)
                {
                    Text = InterpolateString(actionData.Body, data)
                });

            await smtpService.SendEmail(message);
            return new TypedActionHandlerResult<string>()
            {
                Executed = true,
                Result =
                    $"Sent email to {message.To} from {message.From} with subject {message.Subject} and body {message.Body}",
                TypedData = message.ToString()
            };
        }
    }
}