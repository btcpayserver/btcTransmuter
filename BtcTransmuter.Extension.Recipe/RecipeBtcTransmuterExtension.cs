using BtcTransmuter.Abstractions.Extensions;

namespace BtcTransmuter.Extension.Recipe
{
    public class RecipeBtcTransmuterExtension : BtcTransmuterExtension
    {
        public override string Name => "Recipe Plugin";
        public override string Version  => "0.0.1";
        protected override int Priority => 0;
    }
}