using BtcTransmuter.Abstractions;

namespace BtcTransmuter.Extension.Email.Triggers
{
    public class ReceivedEmailTrigger : BaseTrigger<ReceivedEmailTriggerData>
    {
        public override string Name => "Receive Email";
    }
}