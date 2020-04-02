using System;
using System.Collections.Generic;
using System.Net;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using BtcTransmuter.Data.Entities;
using BtcTransmuter.Data.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BtcTransmuter.Auth
{
    public class BasicAuthenticationHandler : AuthenticationHandler<BasicAuthenticationOptions>
    {
        private readonly IOptionsMonitor<IdentityOptions> _identityOptions;
        private readonly SignInManager<User> _signInManager;
        private readonly UserManager<User> _userManager;

        public BasicAuthenticationHandler(
            IOptionsMonitor<IdentityOptions> identityOptions,
            IOptionsMonitor<BasicAuthenticationOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            ISystemClock clock,
            SignInManager<User> signInManager,
            UserManager<User> userManager) : base(options, logger, encoder, clock)
        {
            _identityOptions = identityOptions;
            _signInManager = signInManager;
            _userManager = userManager;
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            string authHeader = Context.Request.Headers["Authorization"];

            if (authHeader == null || !authHeader.StartsWith("Basic ", StringComparison.OrdinalIgnoreCase))
                return AuthenticateResult.NoResult();
            var encodedUsernamePassword = authHeader.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries)[1]?.Trim();
            var decodedUsernamePassword =
                Encoding.UTF8.GetString(Convert.FromBase64String(encodedUsernamePassword)).Split(':');
            var username = decodedUsernamePassword[0];
            var password = decodedUsernamePassword[1];

            var result = await _signInManager.PasswordSignInAsync(username, password, true, true);
            if (!result.Succeeded)
                return AuthenticateResult.Fail(result.ToString());

            var user = await _userManager.FindByNameAsync(username);
            if (!user.Get<UserBlob>().BasicAuth)
            {
                return AuthenticateResult.Fail("This user does not have basic auth enabled.");
            }
            var claims = new List<Claim>()
            {
                new Claim(_identityOptions.CurrentValue.ClaimsIdentity.UserIdClaimType, user.Id),
            };

            return AuthenticateResult.Success(new AuthenticationTicket(
                new ClaimsPrincipal(new ClaimsIdentity(claims, nameof(AuthenticationSchemes.Basic))),
                nameof(AuthenticationSchemes.Basic)));
        }
    }
}