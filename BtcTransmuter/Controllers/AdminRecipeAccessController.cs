using BtcTransmuter.Abstractions.Recipes;
using BtcTransmuter.Data.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace BtcTransmuter.Controllers
{
    [Authorize(Roles = "Admin")]
    [Route("[controller]/{userId}")]
    public class AdminRecipeAccessController : RecipesController
    {
        public AdminRecipeAccessController(IRecipeManager recipeManager, UserManager<User> userManager) : base(
            recipeManager, userManager)
        {
        }

        protected override string GetUserId()
        {
            return ControllerContext.RouteData.Values["userId"].ToString();
        }
    }
}