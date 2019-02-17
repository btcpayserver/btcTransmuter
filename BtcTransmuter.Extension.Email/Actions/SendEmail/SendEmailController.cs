using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using BtcTransmuter.Abstractions.Actions;
using BtcTransmuter.Abstractions.ExternalServices;
using BtcTransmuter.Abstractions.Recipes;
using BtcTransmuter.Abstractions.Triggers;
using BtcTransmuter.Data.Entities;
using BtcTransmuter.Data.Models;
using BtcTransmuter.Extension.Email.Actions.SendEmail;
using BtcTransmuter.Extension.Email.Triggers.ReceivedEmail;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.Caching.Memory;

namespace BtcTransmuter.Extension.Email.ExternalServices
{
    [Route("email-plugin/actions/send-email")]
    [Authorize]
    public class SendEmailController : Controller
    {
        
        private readonly IRecipeManager _recipeManager;
        private readonly IExternalServiceManager _externalServiceManager;
        private readonly UserManager<User> _userManager;
        private readonly IMemoryCache _memoryCache;

        public SendEmailController(
            IRecipeManager recipeManager,
            IExternalServiceManager externalServiceManager,
            UserManager<User> userManager,
            IMemoryCache memoryCache)
        {
            _recipeManager = recipeManager;
            _externalServiceManager = externalServiceManager;
            _userManager = userManager;
            _memoryCache = memoryCache;
        }

        [HttpGet("{identifier}")]
        public async Task<IActionResult> EditData(string identifier)
        {
            var result = await GetRecipeAction(identifier);
            if (result.Error != null)
            {
                return result.Error;
            }

            var smtpServices = await _externalServiceManager.GetExternalServicesData(new ExternalServicesDataQuery()
            {
                Type = new[] {SmtpService.SmtpExternalServiceType},
                UserId = _userManager.GetUserId(User)
            });

            var vm = new SendEmailViewModel()
            {
                ExternalServices = new SelectList(smtpServices, nameof(ExternalServiceData.Id),
                    nameof(ExternalServiceData.Name), result.Data.ExternalServiceId),
            };
            SetValues(result.Data, vm);

            return View(vm);
        }

        private void SetValues(SendEmailViewModel from, RecipeAction to)
        {
            to.ExternalServiceId = from.ExternalServiceId;
            to.RecipeId = from.RecipeId;
            to.Set((SendEmailData) from);
        }

        private void SetValues(RecipeAction from, SendEmailViewModel to)
        {
            to.RecipeId = from.RecipeId;
            to.ExternalServiceId = from.ExternalServiceId;
            var fromData = from.Get<SendEmailData>();
            to.Body = fromData.Body;
            to.Subject = fromData.Subject;
            to.To = fromData.To;
        }

        [HttpPost("{identifier}")]
        public async Task<IActionResult> EditData(string identifier, SendEmailViewModel data)
        {
            var result = await GetRecipeAction(identifier);
            if (result.Error != null)
            {
                return result.Error;
            }

            if (!ModelState.IsValid)
            {
                var pop3Services = await _externalServiceManager.GetExternalServicesData(new ExternalServicesDataQuery()
                {
                    Type = new[] {Pop3Service.Pop3ExternalServiceType},
                    UserId = _userManager.GetUserId(User)
                });


                data.ExternalServices = new SelectList(pop3Services, nameof(ExternalServiceData.Id),
                    nameof(ExternalServiceData.Name), data.ExternalServiceId);
                return View(data);
            }

            var recipeAction = result.Data;
            SetValues(data, recipeAction);

            await _recipeManager.AddOrUpdateRecipeAction(recipeAction);
            return RedirectToAction("EditRecipe", "Recipes", new
            {
                id = recipeAction.RecipeId,
                statusMessage = "Send Email Action Updated"
            });
        }

        private async Task<(IActionResult Error, RecipeAction Data )> GetRecipeAction(string identifier)
        {
            if (!_memoryCache.TryGetValue(identifier, out RecipeAction data))
            {
                return (RedirectToAction("GetServices", "ExternalServices", new
                {
                    statusMessage = "Error:Data could not be found or data session expired"
                }), null);
            }

            var recipe = await _recipeManager.GetRecipe(data.RecipeId, _userManager.GetUserId(User));

            if (recipe == null)
            {
                return (RedirectToAction("GetServices", "ExternalServices", new
                {
                    statusMessage = "Error:Data could not be found or data session expired"
                }), null);
            }

            return (null, data);
        }


        public class SendEmailViewModel : SendEmailData
        {
            public string RecipeId { get; set; }
            public SelectList ExternalServices { get; set; }
            [Required] public string ExternalServiceId { get; set; }
        }
    }
}