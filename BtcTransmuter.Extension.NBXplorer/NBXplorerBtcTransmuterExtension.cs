using System;
using System.Linq;
using BtcTransmuter.Abstractions.Extensions;
using BtcTransmuter.Extension.NBXplorer.HostedServices;
using BtcTransmuter.Extension.NBXplorer.Models;
using BtcTransmuter.Extension.NBXplorer.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NBitcoin;
using NBXplorer;

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
            serviceCollection.AddSingleton(provider =>
            {
                var configuration = provider.GetService<IConfiguration>();
                var section = configuration.GetSection("NBXplorer");
                return new NBXplorerOptions()
                {
                    Uri = section.GetValue<Uri>(nameof(NBXplorerOptions.Uri)),
                    CookieFile = section.GetValue<string>(nameof(NBXplorerOptions.CookieFile)),
                    NetworkType = section.GetValue<NetworkType>(nameof(NBXplorerOptions.NetworkType),
                        NetworkType.Mainnet),
                    Cryptos = section.GetValue<string>(nameof(NBXplorerOptions.Cryptos))?
                        .Replace(" ", "")?
                        .Split(",")?
                        .Distinct().ToArray()
                    
                };
            });
            serviceCollection.AddSingleton<NBXplorerSummaryProvider>();
            serviceCollection.AddSingleton<NBXplorerClientProvider>();
            serviceCollection.AddSingleton<DerivationStrategyFactoryProvider>();
            serviceCollection.AddSingleton<DerivationSchemeParser>();
            serviceCollection.AddSingleton(provider =>
            {
                var options = provider.GetService<NBXplorerOptions>();
                return new NBXplorerNetworkProvider(options.NetworkType);
            });
        }
    }
}