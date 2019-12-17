using System;
using System.Linq;
using BtcTransmuter.Abstractions.Extensions;
using BtcTransmuter.Abstractions.Helpers;
using BtcTransmuter.Extension.NBXplorer.HostedServices;
using BtcTransmuter.Extension.NBXplorer.Models;
using BtcTransmuter.Extension.NBXplorer.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NBitcoin;
using NBXplorer;
using NBXplorer.Models;
using Newtonsoft.Json;

namespace BtcTransmuter.Extension.NBXplorer
{
    public class NBXplorerBtcTransmuterExtension : BtcTransmuterExtension
    {
        public override string Name => "NBXplorer Plugin";

        public override string MenuPartial => "NBXplorerMenuExtension";

        public override void Execute(IServiceCollection serviceCollection)
        {
            base.Execute(serviceCollection);
            serviceCollection.AddSingleton(provider =>
            {
                var configuration = provider.GetService<IConfiguration>();
                return new NBXplorerOptions()
                {
                    Uri = configuration.GetValue<Uri>($"NBXplorer_{nameof(NBXplorerOptions.Uri)}"),
                    UseDefaultCookie = configuration.GetValue($"NBXplorer_{nameof(NBXplorerOptions.UseDefaultCookie)}", 0) == 1,
                    CookieFile = configuration.GetValue<string>($"NBXplorer_{nameof(NBXplorerOptions.CookieFile)}"),
                    NetworkType =
                        configuration.GetValue<NetworkType>($"NBXplorer_{nameof(NBXplorerOptions.NetworkType)}",
                            NetworkType.Regtest),
                    Cryptos = configuration
                        .GetValue<string>($"NBXplorer_{nameof(NBXplorerOptions.Cryptos)}", string.Empty)?
                        .Replace(" ", "")
                        .ToUpperInvariant()?
                        .Split(';')?
                        .Distinct().ToArray()
                };
            });
            serviceCollection.AddSingleton<NBXplorerSummaryProvider>();
            serviceCollection.AddSingleton<NBXplorerClientProvider>();
            serviceCollection.AddSingleton<DerivationSchemeParser>();
            serviceCollection.AddSingleton<NBXplorerPublicWalletProvider>();
            serviceCollection.AddSingleton(provider =>
                new TransmuterInterpolationTypeProvider(typeof(Money), typeof(MoneyExtensions), typeof(MoneyUnit)));
            serviceCollection.AddSingleton(provider =>
            {
                var options = provider.GetService<NBXplorerOptions>();
                return new NBXplorerNetworkProvider(options.NetworkType);
            });
        }
  
    }
}