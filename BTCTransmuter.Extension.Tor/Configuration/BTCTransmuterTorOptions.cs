using Microsoft.Extensions.Configuration;

namespace BTCTransmuter.Extension.Tor.Configuration
{
    public class BTCTransmuterTorOptions
    {
        public string TorrcFile { get; }
        public string TransmuterHiddenServiceName { get; }

        public BTCTransmuterTorOptions(IConfiguration configuration)
        {
            TorrcFile = configuration.GetValue<string>(nameof(TorrcFile), null);
            TransmuterHiddenServiceName = configuration.GetValue(nameof(TransmuterHiddenServiceName), "BTCTransmuter");
        }
    }
}