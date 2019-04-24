using System.Threading.Tasks;
using BtcTransmuter.Abstractions.Recipes;
using BtcTransmuter.Data.Entities;
using BtcTransmuter.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BtcTransmuter.Controllers
{
    [Authorize(Roles = "Admin")]
    [Route("[controller]")]
    public class AdminController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly IRecipeManager _recipeManager;

        public AdminController(UserManager<User> userManager, IRecipeManager recipeManager)
        {
            _userManager = userManager;
            _recipeManager = recipeManager;
        }

        [HttpGet("users")]
        public async Task<IActionResult> Users(string statusMessage)
        {
            return View(
                new UsersViewModel()
                {
                    Users =  await _userManager.Users.ToListAsync(),
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

    }
}