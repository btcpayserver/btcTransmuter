using ExtCore.Infrastructure;

namespace BtcTransmuter.Controllers
{
    public class RemoteAvailableExtension : IExtension
    {
        public string ZipUrl { get; set; }
        public string Name { get; }
        public string Description { get; }
        public string Url { get; }
        public string Version { get; }
        public string Authors { get; }
    }
}