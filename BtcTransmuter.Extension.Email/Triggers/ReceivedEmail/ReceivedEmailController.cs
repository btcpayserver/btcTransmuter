using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using BtcTransmuter.Abstractions.ExternalServices;
using BtcTransmuter.Abstractions.Recipes;
using BtcTransmuter.Data.Entities;
using BtcTransmuter.Data.Models;
using BtcTransmuter.Extension.Email.ExternalServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Caching.Memory;

namespace BtcTransmuter.Extension.Email.Triggers.ReceivedEmail
{
    [Authorize]
    [Route("email-plugin/triggers/received-email")]
    public class ReceivedEmailController : Controller
    {
        private readonly IRecipeManager _recipeManager;
        private readonly IExternalServiceManager _externalServiceManager;
        private readonly UserManager<User> _userManager;
        private readonly IMemoryCache _memoryCache;

        public ReceivedEmailController(
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
            var result = await GetRecipeTrigger(identifier);
            if (result.Error != null)
            {
                return result.Error;
            }

            var pop3Services = await _externalServiceManager.GetExternalServicesData(new ExternalServicesDataQuery()
            {
                Type = new[] {Pop3Service.Pop3ExternalServiceType},
                UserId = _userManager.GetUserId(User)
            });

            var vm = new ReceivedEmailViewModel()
            {
                ExternalServices = new SelectList(pop3Services, nameof(ExternalServiceData.Id),
                    nameof(ExternalServiceData.Name), result.Data.ExternalServiceId),
            };
            SetValues(result.Data, vm);

            return View(vm);
        }

        private void SetValues(ReceivedEmailViewModel from, RecipeTrigger to)
        {
            to.ExternalServiceId = from.ExternalServiceId;
            to.RecipeId = from.RecipeId;
            to.Set((ReceivedEmailTriggerParameters) from);
        }

        private void SetValues(RecipeTrigger from, ReceivedEmailViewModel to)
        {
            to.RecipeId = from.RecipeId;
            to.ExternalServiceId = from.ExternalServiceId;
            var fromData = from.Get<ReceivedEmailTriggerParameters>();
            to.Body = fromData.Body;
            to.Subject = fromData.Subject;
            to.FromEmail = fromData.FromEmail;
            to.BodyComparer = fromData.BodyComparer;
            to.SubjectComparer = fromData.SubjectComparer;
        }

        [HttpPost("{identifier}")]
        public async Task<IActionResult> EditData(string identifier, ReceivedEmailViewModel data)
        {
            var result = await GetRecipeTrigger(identifier);
            if (result.Error != null)
            {
                return result.Error;
            }

            if (!string.IsNullOrEmpty(data.Body) && data.BodyComparer == ReceivedEmailTriggerParameters.FieldComparer.None)
            {
                ModelState.AddModelError(nameof(ReceivedEmailViewModel.BodyComparer), "Please choose a Body filter type");
            } 
            if (!string.IsNullOrEmpty(data.Subject) && data.SubjectComparer == ReceivedEmailTriggerParameters.FieldComparer.None)
            {
                ModelState.AddModelError(nameof(ReceivedEmailViewModel.SubjectComparer), "Please choose a Subject filter type");
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

            var recipeTrigger = result.Data;
            SetValues(data, recipeTrigger);

            await _recipeManager.AddOrUpdateRecipeTrigger(recipeTrigger);
            return RedirectToAction("EditRecipe", "Recipes", new
            {
                id = recipeTrigger.RecipeId,
                statusMessage = "Received Email trigger Updated"
            });
        }

        private async Task<(IActionResult Error, RecipeTrigger Data )> GetRecipeTrigger(string identifier)
        {
            if (!_memoryCache.TryGetValue(identifier, out RecipeTrigger data))
            {
                return (RedirectToAction("GetRecipes", "Recipes", new
                {
                    statusMessage = "Error:Data could not be found or data session expired"
                }), null);
            }

            var recipe = await _recipeManager.GetRecipe(data.RecipeId, _userManager.GetUserId(User));

            if (recipe == null)
            {
                return (RedirectToAction("GetRecipes", "Recipes", new
                {
                    statusMessage = "Error:Data could not be found or data session expired"
                }), null);
            }

            return (null, data);
        }


        public class ReceivedEmailViewModel : ReceivedEmailTriggerParameters
        {
            public string RecipeId { get; set; }
            public SelectList ExternalServices { get; set; }
            [Required] public string ExternalServiceId { get; set; }
        }
    }
}