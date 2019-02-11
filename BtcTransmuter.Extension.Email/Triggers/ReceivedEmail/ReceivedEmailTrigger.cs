using BtcTransmuter.Abstractions;
using BtcTransmuter.Abstractions.Triggers;

namespace BtcTransmuter.Extension.Email.Triggers
{
    public class ReceivedEmailTrigger : BaseTrigger<ReceivedEmailTriggerData>
    {
        public override string Name => "Receive Email";
    }
}