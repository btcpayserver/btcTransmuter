using BtcTransmuter.Abstractions.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace BtcTransmuter.Extension.Lightning
{
    public class LightningBtcTransmuterExtension : BtcTransmuterExtension
    {
        public override string Name => "Lightning Plugin";


        public override void Execute(IServiceCollection serviceCollection)
        {
            base.Execute(serviceCollection);

            serviceCollection.AddSingleton<SocketFactory>();
        }
    }
}