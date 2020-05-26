using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Server.HttpSys;

namespace BtcTransmuter.Auth
{
    public class TransmuterSchemes
    {
        public const string AllSchemes = Local + "," + API;

        public const string API = Basic;
        public const string Basic = nameof(AuthenticationSchemes.Basic);

        //IdentityConstants.ApplicationScheme
        public const string Local = "Identity.Application" + "," +  CookieAuthenticationDefaults.AuthenticationScheme;
    }
}