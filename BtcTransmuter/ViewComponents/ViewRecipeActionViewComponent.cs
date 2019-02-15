using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BtcTransmuter.Abstractions.Actions;
using BtcTransmuter.Abstractions.Extensions;
using BtcTransmuter.Abstractions.ExternalServices;
using BtcTransmuter.Data.Entities;
using BtcTransmuter.Models;
using Microsoft.AspNetCore.Mvc;

namespace BtcTransmuter.Areas.ViewComponents
{
    public class ViewRecipeActionViewComponent : ViewComponent
    {
        private readonly IEnumerable<IActionDescriptor> _actionDescriptors;
        private readonly IEnumerable<IExternalServiceDescriptor> _externalServiceDescriptors;
        private readonly IEnumerable<BtcTransmuterExtension> _extensions;

        public ViewRecipeActionViewComponent(IEnumerable<IActionDescriptor> actionDescriptors,
            IEnumerable<IExternalServiceDescriptor> externalServiceDescriptors)
        {
            _actionDescriptors = actionDescriptors;
            _externalServiceDescriptors = externalServiceDescriptors;
        }

        public async Task<IViewComponentResult> InvokeAsync(RecipeAction recipeAction)
        {
            return View(new ViewRecipeActionViewModel()
            {
                RecipeAction = recipeAction,
                ExternalServiceData = recipeAction.ExternalService,
                ActionDescriptor =
                    _actionDescriptors.Single(descriptor => descriptor.ActionId == recipeAction.ActionId),
                ExternalServiceDescriptor = _externalServiceDescriptors
                    .Single(descriptor => descriptor.ExternalServiceType == recipeAction.ExternalService.Type)
            });
        }
    }
}