using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using BtcTransmuter.Abstractions.ExternalServices;
using BtcTransmuter.Abstractions.Recipes;
using BtcTransmuter.Abstractions.Settings;
using BtcTransmuter.Controllers;
using BtcTransmuter.Data.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BtcTransmuter.Areas.Identity.Pages.Account.Manage
{
    public class DeletePersonalDataModel : PageModel
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly IExternalServiceManager _externalServiceManager;
        private readonly IRecipeManager _recipeManager;
        private readonly ILogger<DeletePersonalDataModel> _logger;
        private readonly ISettingsManager _settingsManager;

        public DeletePersonalDataModel(
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            IExternalServiceManager externalServiceManager,
            IRecipeManager recipeManager,
            ILogger<DeletePersonalDataModel> logger, 
            ISettingsManager settingsManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _externalServiceManager = externalServiceManager;
            _recipeManager = recipeManager;
            _logger = logger;
            _settingsManager = settingsManager;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Required]
            [DataType(DataType.Password)]
            public string Password { get; set; }
        }

        public bool RequirePassword { get; set; }

        public async Task<IActionResult> OnGet()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            RequirePassword = await _userManager.HasPasswordAsync(user);
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            RequirePassword = await _userManager.HasPasswordAsync(user);
            if (RequirePassword)
            {
                if (!await _userManager.CheckPasswordAsync(user, Input.Password))
                {
                    ModelState.AddModelError(string.Empty, "Password not correct.");
                    return Page();
                }
            }

            var services = await _externalServiceManager.GetExternalServicesData(new ExternalServicesDataQuery()
            {
                UserId = user.Id
            });
            
            var recipes = await _recipeManager.GetRecipes(new RecipesQuery()
            {
                UserId = user.Id
            });
            
            foreach (var recipe in recipes)
            {
              await   _recipeManager.RemoveRecipe(recipe.Id);
            }

            foreach (var externalServiceData in services)
            {
               await  _externalServiceManager.RemoveExternalServiceData(externalServiceData.Id);
            }
            var result = await _userManager.DeleteAsync(user);
            var userId = await _userManager.GetUserIdAsync(user);
            if (!result.Succeeded)
            {
                throw new InvalidOperationException($"Unexpected error occurred deleteing user with ID '{userId}'.");
            }

            if (!await _userManager.Users.AnyAsync())
            {
                var settings = await _settingsManager.GetSettings<SystemSettings>(nameof(SystemSettings));
                if (settings.DisableRegistration)
                {
                    settings.DisableRegistration = false;
                    await _settingsManager.SaveSettings(nameof(SystemSettings), settings);
                }
            }

            await _signInManager.SignOutAsync();

            _logger.LogInformation("User with ID '{UserId}' deleted themselves.", userId);

            return Redirect("~/");
        }
    }
}