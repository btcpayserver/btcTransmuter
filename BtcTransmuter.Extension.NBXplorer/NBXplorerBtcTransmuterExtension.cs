using System;
using System.Linq;
using BtcTransmuter.Abstractions.Extensions;
using BtcTransmuter.Extension.NBXplorer.HostedServices;
using BtcTransmuter.Extension.NBXplorer.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NBitcoin;

namespace BtcTransmuter.Extension.NBXplorer
{
    public class NBXplorerBtcTransmuterExtension : BtcTransmuterExtension
    {
        public override string Name => "NBXplorer Plugin";
        public override string Version => "0.0.1";
        protected override int Priority => 0;

        public override string MenuPartial => "NBXplorerMenuExtension";

        public override void Execute(IServiceCollection serviceCollection, IServiceProvider serviceProvider)
        {
            base.Execute(serviceCollection, serviceProvider);
            serviceCollection.AddOptions<NBXplorerOptions>("NBXplorer").Configure<IConfiguration>(
                (options, configuration) =>
                {
                    var section = configuration.GetSection("NBXplorer");
                    options.Uri = section.GetValue<Uri>(nameof(options.Uri));
                    options.CookieFile = section.GetValue<string>(nameof(options.CookieFile));
                    options.NetworkType = section.GetValue<NetworkType>(nameof(options.NetworkType));
                    options.Cryptos = section.GetValue<string>(nameof(options.Cryptos))?
                        .Replace(" ", "")?
                        .Split(",")?
                        .Distinct().ToArray();
                });
            serviceCollection.AddSingleton<NBXplorerSummaryProvider>();
            serviceCollection.AddSingleton<NBXplorerClientProvider>();
        }
    }
}