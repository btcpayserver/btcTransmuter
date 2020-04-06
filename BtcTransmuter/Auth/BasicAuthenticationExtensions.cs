using System.Net;
using Microsoft.AspNetCore.Authentication;

namespace BtcTransmuter.Auth
{
    public static class BasicAuthenticationExtensions
    {
        public static AuthenticationBuilder AddBasicAuth(this AuthenticationBuilder builder)
        {
            return builder.AddScheme<BasicAuthenticationOptions, BasicAuthenticationHandler>(
                nameof(AuthenticationSchemes.Basic),
                o => { });
        }
    }
}