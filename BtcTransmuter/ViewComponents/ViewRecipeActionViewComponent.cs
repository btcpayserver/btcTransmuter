using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BtcTransmuter.Abstractions.Actions;
using BtcTransmuter.Data.Entities;
using BtcTransmuter.Models;
using Microsoft.AspNetCore.Mvc;

namespace BtcTransmuter.ViewComponents
{
    public class ViewRecipeActionViewComponent : ViewComponent
    {
        private readonly IEnumerable<IActionDescriptor> _actionDescriptors;

        public ViewRecipeActionViewComponent(IEnumerable<IActionDescriptor> actionDescriptors)
        {
            _actionDescriptors = actionDescriptors;
        }

        public Task<IViewComponentResult> InvokeAsync(RecipeAction recipeAction)
        {
            return Task.FromResult<IViewComponentResult>(View(new ViewRecipeActionViewModel()
            {
                RecipeAction = recipeAction,
                ExternalServiceData = recipeAction.ExternalService,
                ActionDescriptor =
                    _actionDescriptors.SingleOrDefault(descriptor => descriptor.ActionId == recipeAction.ActionId)
            }));
        }
    }
}