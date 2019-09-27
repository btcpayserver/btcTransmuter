using System;
using BtcTransmuter.Abstractions.Extensions;
using BTCTransmuter.Extension.Tor.Configuration;
using BTCTransmuter.Extension.Tor.Services;
using Microsoft.Extensions.DependencyInjection;

namespace BTCTransmuter.Extension.Tor
{
    public class TorBtcTransmuterExtension : BtcTransmuterExtension
    {
        public override string Name => "Tor Plugin";
        public override string Description => "Allows you to access Transmuter over Tor";
        public override string MenuPartial => "TorOnionLinkPartial";

        public override void Execute(IServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton<BTCTransmuterTorOptions>();
            serviceCollection.AddSingleton<TorServices>();
            base.Execute(serviceCollection);
        }
    }
}