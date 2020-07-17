using System;
using System.ComponentModel.DataAnnotations;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using BtcTransmuter.Data.Entities;
using BtcTransmuter.Data.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace BtcTransmuter.Areas.Identity.Pages.Account.Manage
{
    public class BTCPayAccountLinkModel : PageModel
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly IBtcTransmuterOptions _btcTransmuterOptions;
        private readonly BTCPayAuthService _btcPayAuthService;

        public BTCPayAccountLinkModel(
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            IBtcTransmuterOptions btcTransmuterOptions, BTCPayAuthService btcPayAuthService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _btcTransmuterOptions = btcTransmuterOptions;
            _btcPayAuthService = btcPayAuthService;
        }

        public bool CurrentTokenValid { get; set; }
        [BindProperty] public InputModel Input { get; set; }

        [TempData] public string StatusMessage { get; set; }

        public class InputModel
        {
            [Display(Name = "Access Token")] public string AccessToken { get; set; }
        }

        public async Task<IActionResult> OnGetAsync()
        {
            if (_btcTransmuterOptions.BTCPayAuthServer == null)
            {
                return RedirectToPage("./ChangePassword");
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            var blob = user.Get<UserBlob>();
            Input = new InputModel()
            {
                AccessToken = blob.BTCPayAuthDetails.AccessToken
            };
            if (!string.IsNullOrEmpty(Input.AccessToken))
            {
                var currentToken = await _btcPayAuthService.CheckToken(user);
                CurrentTokenValid = currentToken != null && currentToken.Id == blob.BTCPayAuthDetails.UserId;
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (_btcTransmuterOptions.BTCPayAuthServer == null)
            {
                return RedirectToPage("./ChangePassword");
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            var blob = user.Get<UserBlob>();
            if (_btcTransmuterOptions.DisableInternalAuth && string.IsNullOrEmpty(Input.AccessToken))
            {
                ModelState.AddModelError("Input.AccessToken", "Access token is required.");
            }
            else if (_btcTransmuterOptions.DisableInternalAuth && !string.IsNullOrEmpty(Input.AccessToken))
            {
                var response = await _btcPayAuthService.CheckToken(user);
                CurrentTokenValid = response != null;
                if (CurrentTokenValid)
                {
                    blob.BTCPayAuthDetails.AccessToken = Input.AccessToken;
                    blob.BTCPayAuthDetails.UserId = response.Id;
                    user.Set(blob);

                    await _userManager.UpdateAsync(user);
                }
                else if (!CurrentTokenValid && _btcTransmuterOptions.DisableInternalAuth)
                {
                    ModelState.AddModelError("Input.AccessToken", "Invalid Access token.");
                }
            }

            if (!ModelState.IsValid)
            {
                return Page();
            }
            return RedirectToPage();
        }
    }
}