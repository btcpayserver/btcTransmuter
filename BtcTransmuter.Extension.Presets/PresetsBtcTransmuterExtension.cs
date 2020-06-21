using BtcTransmuter.Abstractions.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace BtcTransmuter.Extension.Presets
{
    public class PresetsBtcTransmuterExtension : BtcTransmuterExtension
    {
        public override string Name => "Presets Plugin";
        public override string MenuPartial => "PresetsMenuExtension";

        public override void Execute(IServiceCollection serviceCollection)
        {
            base.Execute(serviceCollection);
            serviceCollection.AddTransient<ITransmuterPreset, PaymentForwarderController>();
            serviceCollection.AddTransient<ITransmuterPreset, BTCPayEmailReceiptsController>();
            serviceCollection.AddTransient<ITransmuterPreset, FiatExchangeConversionController>();
            serviceCollection.AddTransient<ITransmuterPreset, DCAController>();
        }
    }
}