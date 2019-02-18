using BtcTransmuter.Abstractions.Extensions;

namespace BtcTransmuter.Extension.Email
{

    
    public class EmailBtcTransmuterExtension : BtcTransmuterExtension
    {
        public override string Name => "Emails Plugin";
        public override string Version  => "0.0.1";
        protected override int Priority => 0;
        public override string[] Scripts => new string[0];
        public override string[] Stylesheets => new string[0];
    }
}