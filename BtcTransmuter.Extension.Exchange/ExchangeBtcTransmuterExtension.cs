using System;
using BtcTransmuter.Abstractions.Extensions;

namespace BtcTransmuter.Extension.Exchange
{
    public class ExchangeBtcTransmuterExtension : BtcTransmuterExtension
    {
        public override string Name => "Exchange Plugin";
        public override string Version  => "0.0.1";
        protected override int Priority => 0;
    }
}