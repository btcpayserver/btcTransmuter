using BtcTransmuter.Abstractions.Extensions;

namespace BtcTransmuter.Extension.Webhook
{
    public class WebhookBtcTransmuterExtension : BtcTransmuterExtension
    {
        public override string Name => "Webhook Plugin";
        public override string Version  => "0.0.1";
        protected override int Priority => 0;
    }
}