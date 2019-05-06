using System.Collections.Generic;
using BtcTransmuter.Abstractions.Extensions;
using BTCPayServer.Lightning.JsonConverters;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace BtcTransmuter.Extension.Lightning
{
    public class LightningBtcTransmuterExtension : BtcTransmuterExtension
    {
        public override string Name => "Lightning Plugin";
        public override IEnumerable<JsonConverter> JsonConverters { get; set; } = new List<JsonConverter>()
        {
            new LightMoneyJsonConverter()
        };
        public override void Execute(IServiceCollection serviceCollection)
        {
            base.Execute(serviceCollection);

            serviceCollection.AddSingleton<SocketFactory>();
        }
    }
}