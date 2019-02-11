using System.Net.Mail;
using System.Threading.Tasks;
using BtcTransmuter.Abstractions.Actions;
using BtcTransmuter.Data;
using BtcTransmuter.Extension.Email.ExternalServices;

namespace BtcTransmuter.Extension.Email.Actions
{
    public class SendEmailData
    {
        public string From { get; set; }
        public string To { get; set; }
        public string Email { get; set; }
        public string Body { get; set; }
        public string Subject { get; set; }
    }

    public class SendEmailDataActionHandler : BaseActionHandler<SendEmailData>
    {
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