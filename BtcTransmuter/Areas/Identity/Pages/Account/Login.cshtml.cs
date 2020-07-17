using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using BtcTransmuter.Data.Entities;
using BtcTransmuter.Data.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace BtcTransmuter.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class LoginModel : PageModel
    {
        private readonly BTCPayAuthService _btcPayAuthService;
        private readonly SignInManager<User> _signInManager;
        private readonly ILogger<LoginModel> _logger;
        private readonly IBtcTransmuterOptions _btcTransmuterOptions;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly UserManager<User> _userManager;

        public LoginModel(BTCPayAuthService btcPayAuthService,SignInManager<User> signInManager, ILogger<LoginModel> logger, IBtcTransmuterOptions btcTransmuterOptions, IHttpClientFactory httpClientFactory, UserManager<User> userManager)
        {
            _btcPayAuthService = btcPayAuthService;
            _signInManager = signInManager;
            _logger = logger;
            _btcTransmuterOptions = btcTransmuterOptions;
            _httpClientFactory = httpClientFactory;
            _userManager = userManager;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        public string ReturnUrl { get; set; }

        [TempData]
        public string ErrorMessage { get; set; }

        public class InputModel
        {
            [Required]
            [EmailAddress]
            public string Email { get; set; }

            [Required]
            [DataType(DataType.Password)]
            public string Password { get; set; }

            [Display(Name = "Remember me?")]
            public bool RememberMe { get; set; }
        }

        public async Task OnGetAsync(string returnUrl = null)
        {
            if (!string.IsNullOrEmpty(ErrorMessage))
            {
                ModelState.AddModelError(string.Empty, ErrorMessage);
            }

            returnUrl = returnUrl ?? Url.Content("~/");

            // Clear the existing external cookie to ensure a clean login process
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            ReturnUrl = returnUrl;
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl = returnUrl ?? Url.Content("~/");

            if (ModelState.IsValid)
            {

                var user = await _btcPayAuthService.LoginAndRegisterIfNeeded(Input.Email, Input.Password);
                if (user != null)
                {
                    await _signInManager.SignInAsync(user, Input.RememberMe);
                    _logger.LogInformation("User logged in using BTCPay.");
                    return LocalRedirect(returnUrl);
                }
                else if (!_btcTransmuterOptions.DisableInternalAuth)
                {
                    var result = await _signInManager.PasswordSignInAsync(Input.Email, Input.Password, Input.RememberMe,
                        lockoutOnFailure: true);
                    if (result.Succeeded)
                    {
                        _logger.LogInformation("User logged in.");
                        return LocalRedirect(returnUrl);
                    }

                    if (result.RequiresTwoFactor)
                    {
                        return RedirectToPage("./LoginWith2fa",
                            new {ReturnUrl = returnUrl, RememberMe = Input.RememberMe});
                    }

                    if (result.IsLockedOut)
                    {
                        _logger.LogWarning("User account locked out.");
                        return RedirectToPage("./Lockout");
                    }
                }

                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            }

            // If we got this far, something failed, redisplay form
            return Page();
        }
    }
    
    public class GetCurrentUserResponse    {

        [JsonProperty("id")]
        public string Id { get; set; } 

        [JsonProperty("email")]
        public string Email { get; set; } 

        [JsonProperty("emailConfirmed")]
        public bool EmailConfirmed { get; set; } 

        [JsonProperty("requiresEmailConfirmation")]
        public bool RequiresEmailConfirmation { get; set; }

        public override string ToString()
        {
            return $"{Id}{Email}";
        }
    }
}
