using System;
using System.Threading;
using System.Threading.Tasks;
using BTCTransmuter.Extension.Tor.Configuration;
using BTCTransmuter.Extension.Tor.Services;
using Microsoft.Extensions.Hosting;

namespace BTCTransmuter.Extension.Tor.HostedServices
{
    public class TorServicesHostedService : IHostedService
    {
        private readonly BTCTransmuterTorOptions _btcTransmuterTorOptions;
        private readonly TorServices _torServices;

        public TorServicesHostedService(BTCTransmuterTorOptions btcTransmuterTorOptions, TorServices torServices)
        {
            _btcTransmuterTorOptions = btcTransmuterTorOptions;
            _torServices = torServices;
        }
        public Task StartAsync(CancellationToken cancellationToken)
        {
            if (!string.IsNullOrEmpty(_btcTransmuterTorOptions.TorrcFile))
            {
                _ = ContinuouslyMonitorTorServices(cancellationToken);
            }

            return Task.CompletedTask;
        }

        private async Task ContinuouslyMonitorTorServices(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                _torServices.Refresh();
                await Task.Delay(TimeSpan.FromMinutes(2), cancellationToken);
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
