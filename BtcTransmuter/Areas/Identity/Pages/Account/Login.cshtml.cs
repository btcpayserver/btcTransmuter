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

namespace BtcTransmuter.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class LoginModel : PageModel
    {
        private readonly SignInManager<User> _signInManager;
        private readonly ILogger<LoginModel> _logger;
        private readonly IBtcTransmuterOptions _btcTransmuterOptions;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly UserManager<User> _userManager;

        public LoginModel(SignInManager<User> signInManager, ILogger<LoginModel> logger, IBtcTransmuterOptions btcTransmuterOptions, IHttpClientFactory httpClientFactory, UserManager<User> userManager)
        {
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
                if (_btcTransmuterOptions.BTCPayAuthServer != null)
                {
                    var client = _httpClientFactory.CreateClient("BTCPayAuthServer");
                    var fetchUserId = new Uri(_btcTransmuterOptions.BTCPayAuthServer, "api/v1/users/me");
                    var request = new HttpRequestMessage(HttpMethod.Get, fetchUserId);
                    request.Headers.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.UTF8.GetBytes($"{Input.Email}:{Input.Password}")));
                    var response = await client.SendAsync(request);
                    if (!response.IsSuccessStatusCode && _btcTransmuterOptions.DisableInternalAuth)
                    {
                        ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                        return Page();
                    }else if (response.IsSuccessStatusCode)
                    {
                        var parsedResponse =
                            JsonConvert.DeserializeObject<GetCurrentUserResponse>(
                                await response.Content.ReadAsStringAsync());
                        if (parsedResponse.RequiresEmailConfirmation && !parsedResponse.EmailConfirmed)
                        {
                            ModelState.AddModelError(string.Empty, "You need to verify your email on BTCPay Server first.");
                            return Page();
                        }
                        var matchedUser = _userManager.Users.SingleOrDefault(user =>
                            user.Get<UserBlob>().BTCPayAuthDetails.UserId == parsedResponse.Id);
                        bool generateKey;
                        if (matchedUser == null)
                        {
                            //create account
                            generateKey = true;
                            matchedUser = new User()
                            {
                                Email = parsedResponse.Email,
                                Id = parsedResponse.Id,
                                UserName = parsedResponse.Email,
                            };
                            matchedUser.Set(new UserBlob()
                            {
                                BTCPayAuthDetails = new BTCPayAuthDetails()
                                {
                                    UserId = parsedResponse.Id
                                }
                            });
                            await _userManager.CreateAsync(matchedUser);
                        }
                        else
                        {
                            var blob = matchedUser.Get<UserBlob>();
                            request.Headers.Authorization = new AuthenticationHeaderValue("token ", blob.BTCPayAuthDetails.AccessToken);
                            response = await client.SendAsync(request);
                            if (!response.IsSuccessStatusCode)
                            {
                                //need to regenerate the api key
                                
                                generateKey = true;
                            }
                            else
                            {
                                await _signInManager.SignInAsync(matchedUser, Input.RememberMe);
                                _logger.LogInformation("User logged in using BTCPay.");
                                return LocalRedirect(returnUrl);
                            }
                        }

                        if (generateKey && matchedUser != null)
                        {
                            request.Method = HttpMethod.Post;
                            request.RequestUri = new Uri(_btcTransmuterOptions.BTCPayAuthServer, "/api/v1/api-keys");
                            request.Headers.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.UTF8.GetBytes($"{Input.Email}:{Input.Password}")));
                            request.Content = new StringContent(JsonConvert.SerializeObject(new
                            {
                                label = "transmuter login access token",
                                permissions = new string[0]
                            }), Encoding.UTF8, "application/json");
                            
                            response = await client.SendAsync(request);
                            if (!response.IsSuccessStatusCode && _btcTransmuterOptions.DisableInternalAuth)
                            {
                                    ModelState.AddModelError(string.Empty, "Could not generate access token from BTCPay Server");
                                    return Page();
                            }
                            else if(response.IsSuccessStatusCode)
                            {
                                var accessTokenResponse =
                                    JsonConvert.DeserializeObject<dynamic>((await response.Content.ReadAsStringAsync()));
                                var blob = matchedUser.Get<UserBlob>();
                                blob.BTCPayAuthDetails.AccessToken = accessTokenResponse.apiKey;
                                matchedUser.Set(blob);
                                await _userManager.UpdateAsync(matchedUser);
                            }
                        }
                    }
                }
                
                
                var result = await _signInManager.PasswordSignInAsync(Input.Email, Input.Password, Input.RememberMe, lockoutOnFailure: true);
                if (result.Succeeded)
                {
                    _logger.LogInformation("User logged in.");
                    return LocalRedirect(returnUrl);
                }
                if (result.RequiresTwoFactor)
                {
                    return RedirectToPage("./LoginWith2fa", new { ReturnUrl = returnUrl, RememberMe = Input.RememberMe });
                }
                if (result.IsLockedOut)
                {
                    _logger.LogWarning("User account locked out.");
                    return RedirectToPage("./Lockout");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                    return Page();
                }
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

    }
}
