using Microsoft.Extensions.DependencyInjection;

namespace BtcTransmuter.Abstractions.Helpers
{
    public static class DependencyHelper
    {
        public static IServiceScopeFactory ServiceScopeFactory { get; set; }
    }
}


