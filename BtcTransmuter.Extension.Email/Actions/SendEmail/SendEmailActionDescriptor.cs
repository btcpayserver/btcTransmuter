using BtcTransmuter.Abstractions.Actions;
using BtcTransmuter.Abstractions.Triggers;

namespace BtcTransmuter.Extension.Email.Actions.SendEmail
{
    public class SendEmailActionDescriptor: IActionDescriptor
    {
        public string ActionId => "SendEmail";
        string IActionDescriptor.Name => "Send Email";
        string IActionDescriptor.Description =>
            "Send an email using an smtp external service";

        public string ViewPartial => "ViewSendEmailAction";
    }
}