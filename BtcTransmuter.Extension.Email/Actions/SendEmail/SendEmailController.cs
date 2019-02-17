using System;
using System.Linq;
using System.Threading.Tasks;
using BtcTransmuter.Abstractions.ExternalServices;
using BtcTransmuter.Data.Entities;
using BtcTransmuter.Data.Models;
using BtcTransmuter.Extension.Email.Actions.SendEmail;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.Caching.Memory;

namespace BtcTransmuter.Extension.Email.ExternalServices
{
    [Route("email-plugin/actions/send-email")]
    [Authorize]
    public class SendEmailController : Controller
    {
        private readonly IExternalServiceManager _externalServiceManager;
        private readonly UserManager<User> _userManager;
        private readonly IMemoryCache _memoryCache;

        public SendEmailController(IExternalServiceManager externalServiceManager, UserManager<User> userManager,
            IMemoryCache memoryCache)
        {
            _externalServiceManager = externalServiceManager;
            _userManager = userManager;
            _memoryCache = memoryCache;
        }

        [HttpGet("{identifier}")]
        public async Task<IActionResult> EditData(string identifier)
        {
            throw new NotImplementedException();
//            var result = await GetRecipeAction(identifier);
//            if (result.Error != null)
//            {
//                return result.Error;
//            }
//
//            
//            return View(pop3Service.Data);
        }

        [HttpPost("{identifier}")]
        public async Task<IActionResult> EditData(string identifier, SendEmailData data)
        {
            throw new NotImplementedException();
//            var result = await GetRecipeAction(identifier);
//            if (result.Error != null)
//            {
//                return result.Error;
//            }
//
//            var externalServiceData = result.Data;
//            if (!ModelState.IsValid)
//            {
//                return View(data);
//            }
//
//            externalServiceData.Set(data);
//
//            var pop3Service = new Pop3Service(externalServiceData);
//            var testConnection = await pop3Service.CreateClientAndConnect();
//            if (testConnection == null)
//            {
//                ModelState.AddModelError(string.Empty, "Could not connect successfully");
//
//                return View(data);
//            }
//
//            testConnection.Dispose();
//
//
//            await _externalServiceManager.AddOrUpdateExternalServiceData(externalServiceData);
//            return RedirectToAction("EditExternalService", "ExternalServices", new
//            {
//                id = externalServiceData.Id,
//                statusMessage = "Pop3 Data updated"
//            });
        }
        
        private async Task<(IActionResult Error, SendEmailData Data )> GetRecipeAction(string identifier)
        {
            throw new NotImplementedException();
//            RecipeAction data = null;
//            if (identifier.StartsWith("new"))
//            {
//                if (!_memoryCache.TryGetValue(identifier, out data))
//                {
//                    return (RedirectToAction("GetServices", "ExternalServices", new
//                    {
//                        statusMessage = "Error:Data could not be found or data session expired"
//                    }), null);
//                }
//                if (data.UserId != _userManager.GetUserId(User))
//                {
//                    return (RedirectToAction("GetServices", "ExternalServices", new
//                    {
//                        statusMessage = "Error:Data could not be found or data session expired"
//                    }), null);
//                }
//            }
//            else
//            {
//                var services = await _externalServiceManager.GetExternalServicesData(new ExternalServicesDataQuery()
//                {
//                    UserId = _userManager.GetUserId(User),
//                    Type = new string[] {Pop3Service.Pop3ExternalServiceType},
//                    ExternalServiceId = identifier
//                });
//                if (!services.Any())
//                {
//                    return (
//                        RedirectToAction("GetServices", "ExternalServices", new
//                        {
//                            statusMessage = "Error:Data could not be found"
//                        }), null);
//                }
//
//                data = services.First();
//            }
//
//            return (null, data);
        }
    }
}