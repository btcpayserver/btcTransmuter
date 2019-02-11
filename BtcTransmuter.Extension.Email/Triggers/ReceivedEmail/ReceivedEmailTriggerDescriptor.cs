using BtcTransmuter.Abstractions.Triggers;

namespace BtcTransmuter.Extension.Email.Triggers.ReceivedEmail
{
    public class ReceivedEmailTriggerDescriptor: ITriggerDescriptor
    {
        public string TriggerId => new ReceivedEmailTrigger().Id;
        public string Name => "Receive Email";
        public string Description =>
            "Trigger a recipe by receiving a specifically formatted email through a pop3 external service.";
    }
}