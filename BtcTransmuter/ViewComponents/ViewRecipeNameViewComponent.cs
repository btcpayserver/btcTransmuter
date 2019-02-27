using System.Threading.Tasks;
using BtcTransmuter.Abstractions.Recipes;
using Microsoft.AspNetCore.Mvc;

namespace BtcTransmuter.ViewComponents
{
    public class ViewRecipeNameViewComponent : ViewComponent
    {
        private readonly IRecipeManager _recipeManager;

        public ViewRecipeNameViewComponent(IRecipeManager recipeManager)
        {
            _recipeManager = recipeManager;
        }

        public async Task<IViewComponentResult> InvokeAsync(string recipeId)
        {
            return View("Default", await _recipeManager.GetRecipeName(recipeId));
        }
    }
}