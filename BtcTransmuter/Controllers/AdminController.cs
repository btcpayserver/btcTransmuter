using System.Threading.Tasks;
using BtcTransmuter.Abstractions.Recipes;
using BtcTransmuter.Abstractions.Settings;
using BtcTransmuter.Auth;
using BtcTransmuter.Data.Entities;
using BtcTransmuter.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BtcTransmuter.Controllers
{
    [Authorize(Roles = "Admin", AuthenticationSchemes = TransmuterSchemes.Local)]
    [Route("[controller]")]
    public class AdminController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly IRecipeManager _recipeManager;
        private readonly ISettingsManager _settingsManager;

        public AdminController(UserManager<User> userManager, IRecipeManager recipeManager, ISettingsManager settingsManager)
        {
            _userManager = userManager;
            _recipeManager = recipeManager;
            _settingsManager = settingsManager;
        }

        [HttpGet("users")]
        public async Task<IActionResult> Users(string statusMessage)
        {
            var users = await _userManager.Users.ToListAsync();
            return View(
                new UsersViewModel()
                {
                    Users = users,
                    StatusMessage = statusMessage
                }
            );
        }
        
        [HttpGet("users/{userId}/recipes")]
        public virtual async Task<IActionResult> UserRecipes(string userId, [FromQuery] string statusMessage = null)
        {
            var recipes = await _recipeManager.GetRecipes(new RecipesQuery()
            {
                UserId = userId
            });

            return View(new GetRecipesViewModel()
            {
                StatusMessage = statusMessage,
                Recipes = recipes
            });
        }
        
        
        [HttpGet("SystemSettings")]
        public async Task<IActionResult> EditSettings(string statusMessage)
        {
            var currentSettings = await  _settingsManager.GetSettings<SystemSettings>(nameof(SystemSettings));
            return View(
                new EditSystemSettingsViewModel()
                {
                    StatusMessage = statusMessage,
                    DisableRegistration = currentSettings.DisableRegistration
                }
            );
        }
        
        [HttpPost("SystemSettings")]
        public async Task<IActionResult> EditSettings(EditSystemSettingsViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                return View(vm);
            }

            await _settingsManager.SaveSettings<SystemSettings>(nameof(SystemSettings), vm);
            
            return RedirectToAction(nameof(EditSettings), new
            {
                StatusMessage = "Settings have been updated."
            });
        }

    }
}