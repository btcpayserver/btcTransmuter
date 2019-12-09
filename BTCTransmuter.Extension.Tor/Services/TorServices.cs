using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BTCTransmuter.Extension.Tor.Configuration;
using BTCTransmuter.Extension.Tor.Models;
using Microsoft.Extensions.Logging;

namespace BTCTransmuter.Extension.Tor.Services
{
    public class TorServices
    {
        private readonly BTCTransmuterTorOptions _transmuterTorOptions;
        private readonly ILogger<TorServices> _logger;

        public TorServices(BTCTransmuterTorOptions transmuterTorOptions, ILogger<TorServices> logger)
        {
            _transmuterTorOptions = transmuterTorOptions;
            _logger = logger;
        }

        public TorService[] Services { get; internal set; } = Array.Empty<TorService>();

        public TorService TransmuterTorService
        {
            get
            {
                if (string.IsNullOrEmpty(_transmuterTorOptions.TransmuterHiddenServiceName))
                {
                    return null;
                }

                return Services.SingleOrDefault(service =>
                    service.Name.Equals(_transmuterTorOptions.TransmuterHiddenServiceName,
                        StringComparison.InvariantCultureIgnoreCase));
            }
        }

        internal void Refresh()
        {
            if (string.IsNullOrEmpty(_transmuterTorOptions.TorrcFile) || !File.Exists(_transmuterTorOptions.TorrcFile))
            {
                if (!string.IsNullOrEmpty(_transmuterTorOptions.TorrcFile))
                    _logger.LogWarning("Torrc file is not found");
                Services = Array.Empty<TorService>();
                return;
            }
            List<TorService> result = new List<TorService>();
            try
            {
                var torrcContent = File.ReadAllText(_transmuterTorOptions.TorrcFile);
                if (!Torrc.TryParse(torrcContent, out var torrc))
                {
                    _logger.LogWarning("Torrc file could not be parsed");
                    Services = Array.Empty<TorService>();
                    return;
                }

                var services = torrc.ServiceDirectories.SelectMany(d => d.ServicePorts.Select(p => (Directory: new DirectoryInfo(d.DirectoryPath), VirtualPort: p.VirtualPort)))
                .Select(d => (ServiceName: d.Directory.Name,
                              ReadingLines: System.IO.File.ReadAllLines(Path.Combine(d.Directory.FullName, "hostname")),
                              VirtualPort: d.VirtualPort))
                .ToArray();
                foreach (var service in services)
                {
                    try
                    {
                        var onionHost = (service.ReadingLines)[0].Trim();
                        var torService = new TorService()
                        {
                            Name = service.ServiceName,
                            OnionHost = $"http://{onionHost}",
                            VirtualPort = service.VirtualPort
                        };
                        result.Add(torService);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, $"Error while reading hidden service {service.ServiceName} configuration");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, $"Error while reading torrc file");
            }
            Services = result.ToArray();
        }
    }
}
