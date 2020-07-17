using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using BtcTransmuter.Areas.Identity.Pages.Account;
using BtcTransmuter.Data.Entities;
using BtcTransmuter.Data.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;


namespace BtcTransmuter
{
    public class BTCPayAuthService
    {
        private readonly UserManager<User> _userManager;
        private readonly IBtcTransmuterOptions _btcTransmuterOptions;
        private readonly IHttpClientFactory _httpClientFactory;

        public BTCPayAuthService(UserManager<User> userManager, IBtcTransmuterOptions btcTransmuterOptions, IHttpClientFactory httpClientFactory)
        {
            _userManager = userManager;
            _btcTransmuterOptions = btcTransmuterOptions;
            _httpClientFactory = httpClientFactory;
        }

        public async Task<User> LoginAndRegisterIfNeeded(string user, string pass)
        {
            if (_btcTransmuterOptions.BTCPayAuthServer is null)
            {
                return null;
            }

            var response = await BasicAuthLogin(user, pass);
            if (response == null)
            {
                return null;
            }
            var matchedUser = await FindUserByBTCPayUserId(response.Id);
            if (matchedUser == null)
            {
                var key = await GenerateKey(user, pass);
                if (string.IsNullOrEmpty(key))
                {
                    return null;
                }
                //create account
                matchedUser = new User()
                {
                    Email = response.Email,
                    Id = response.Id,
                    UserName = response.Email,
                };
                matchedUser.Set(new UserBlob()
                {
                    BTCPayAuthDetails = new BTCPayAuthDetails()
                    {
                        UserId = response.Id,
                        AccessToken = key
                    }
                });
                if ((await _userManager.CreateAsync(matchedUser)).Succeeded)
                {
                    if (await _userManager.Users.CountAsync() == 1)
                    {
                        await _userManager.AddToRoleAsync(matchedUser, "Admin");
                    }
                }
                else
                {
                    return null;
                }
            }
            else
            {
                var tokenResponse = await CheckToken(matchedUser);
                
                if (!(tokenResponse?.ToString()?.Equals(response.ToString()) is true) && await GenerateKeyAndSet(user, pass, matchedUser))
                {
                    await _userManager.UpdateAsync(matchedUser);
                }else if (!(tokenResponse?.ToString()?.Equals(response.ToString()) is true))
                {
                    return null;
                }
            }

            return matchedUser;
        }

        private async Task<bool> GenerateKeyAndSet(string user, string pass, User matchedUser)
        {
            var key = await GenerateKey(user, pass);
            if (string.IsNullOrEmpty(key))
            {
                return false;
            }

            var blob = matchedUser.Get<UserBlob>();
            blob.BTCPayAuthDetails.AccessToken  = key;
            if (_btcTransmuterOptions.DisableInternalAuth || !await _userManager.HasPasswordAsync(matchedUser))
            {
                matchedUser.Email = user;
                matchedUser.UserName= user;
            }
            matchedUser.Set(blob);
            return true;
        }
        public async Task<string> GenerateKey(string user, string pass)
        {
            var client = _httpClientFactory.CreateClient("BTCPayAuthServer");
            var request = new HttpRequestMessage(HttpMethod.Post, new Uri(_btcTransmuterOptions.BTCPayAuthServer, "/api/v1/api-keys"));
            request.Headers.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.UTF8.GetBytes($"{user}:{pass}")));
            request.Content = new StringContent(JsonConvert.SerializeObject(new
            {
                label = "transmuter login access token",
                permissions = new[]{ "btcpay.user.canmodifyprofile"}
            }), Encoding.UTF8, "application/json");
                            
            var response = await client.SendAsync(request);

            if(response.IsSuccessStatusCode)
            {
                var accessTokenResponse =
                    JsonConvert.DeserializeObject<JObject>((await response.Content.ReadAsStringAsync()));
                return accessTokenResponse["apiKey"].Value<string>();
            }

            return null;
        }
        public async Task<User> FindUserByBTCPayUserId(string userId)
        {
            return _userManager.Users.AsEnumerable().SingleOrDefault(user =>
                user.Get<UserBlob>().BTCPayAuthDetails.UserId == userId);
        }

        public async Task<GetCurrentUserResponse> BasicAuthLogin(string user, string pass)
        {
            if (_btcTransmuterOptions.BTCPayAuthServer is null)
            {
                return null;
            }
            
            var client = _httpClientFactory.CreateClient("BTCPayAuthServer");
            var fetchUserId = new Uri(_btcTransmuterOptions.BTCPayAuthServer, "api/v1/users/me");
            var request = new HttpRequestMessage(HttpMethod.Get, fetchUserId);
            request.Headers.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.UTF8.GetBytes($"{user}:{pass}")));
            var response = await client.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }
            return JsonConvert.DeserializeObject<GetCurrentUserResponse>(
                    await response.Content.ReadAsStringAsync());
        }

        public async Task<GetCurrentUserResponse> CheckToken(User user)
        {
            if (user == null)
            {
                return null;
            }
            var blob = user.Get<UserBlob>();
            return await CheckToken(blob.BTCPayAuthDetails.AccessToken);
        }
        public async Task<GetCurrentUserResponse> CheckToken(string token)
        {
            if (_btcTransmuterOptions.BTCPayAuthServer is null)
            {
                return null;
            }
            
            var client = _httpClientFactory.CreateClient("BTCPayAuthServer");

            if (string.IsNullOrEmpty(token))
            {
                return null;
            }
            
            var fetchUserId = new Uri(_btcTransmuterOptions.BTCPayAuthServer, "api/v1/users/me");
            var request = new HttpRequestMessage(HttpMethod.Get, fetchUserId);
            request.Headers.Authorization = new AuthenticationHeaderValue("token", token);
            var response = await client.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }
            return JsonConvert.DeserializeObject<GetCurrentUserResponse>(
                await response.Content.ReadAsStringAsync());
        }
        
        
    }
}