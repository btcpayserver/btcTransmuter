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