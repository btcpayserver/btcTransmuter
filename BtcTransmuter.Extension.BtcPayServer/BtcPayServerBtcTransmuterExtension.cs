using BtcTransmuter.Abstractions.Extensions;

namespace BtcTransmuter.Extension.Email
{
    public class BtcPayServerBtcTransmuterExtension : BtcTransmuterExtension
    {
        public override string Name => "BtcPayServer Plugin";
        public override string Version  => "0.0.1";
        protected override int Priority => 0;
    }
}