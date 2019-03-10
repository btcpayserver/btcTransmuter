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

        public override string MenuPartial => "NBXplorerMenuExtension";

        public override void Execute(IServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton(provider =>
            {
                var configuration = provider.GetService<IConfiguration>();
                return new NBXplorerOptions()
                {
                    Uri = configuration.GetValue<Uri>($"NBXplorer_{nameof(NBXplorerOptions.Uri)}"),
                    CookieFile = configuration.GetValue<string>($"NBXplorer_{nameof(NBXplorerOptions.CookieFile)}"),
                    NetworkType =
                        configuration.GetValue<NetworkType>($"NBXplorer_{nameof(NBXplorerOptions.NetworkType)}",
                            NetworkType.Regtest),
                    Cryptos = configuration
                        .GetValue<string>($"NBXplorer_{nameof(NBXplorerOptions.Cryptos)}", string.Empty)?
                        .Replace(" ", "")?
                        .Split(',')?
                        .Distinct().ToArray()
                };
            });
            serviceCollection.AddSingleton<NBXplorerSummaryProvider>();
            serviceCollection.AddSingleton<NBXplorerClientProvider>();
            serviceCollection.AddSingleton<DerivationStrategyFactoryProvider>();
            serviceCollection.AddSingleton<DerivationSchemeParser>();
            serviceCollection.AddSingleton<NBXplorerPublicWalletProvider>();
            serviceCollection.AddSingleton(provider =>
            {
                var options = provider.GetService<NBXplorerOptions>();
                return new NBXplorerNetworkProvider(options.NetworkType);
            });
        }
    }
}