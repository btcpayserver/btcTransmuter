using Microsoft.AspNetCore.Http;

namespace BtcTransmuter.Abstractions.Extensions
{
    public static class RequestExtensions
    {
        public static string GetAbsoluteRoot(this HttpRequest request)
        {
            return string.Concat(
                request.Scheme,
                "://",
                request.Host.ToUriComponent(),
                request.PathBase.ToUriComponent());
        }
    }
}