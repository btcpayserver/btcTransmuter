using System;
using System.Threading.Tasks;
using BtcTransmuter.Abstractions.Actions;
using BtcTransmuter.Abstractions.ExternalServices;
using BtcTransmuter.Abstractions.Helpers;
using BtcTransmuter.Abstractions.Recipes;
using BtcTransmuter.Data.Entities;
using BtcTransmuter.Extension.DynamicService.ExternalServices.DynamicService;
using Microsoft.Extensions.DependencyInjection;

namespace BtcTransmuter.Extension.DynamicServices
{
    public static class RecipeActionExtensions
    {
        public static async Task<ExternalServiceData> GetExternalService(this RecipeAction recipeAction)
        {
            if (recipeAction.ExternalService == null || !recipeAction.ExternalService.Type.Equals(
                    DynamicServiceService.DynamicServiceServiceType, StringComparison.InvariantCultureIgnoreCase))
                return recipeAction.ExternalService;


            using (var scope = DependencyHelper.ServiceScopeFactory.CreateScope())
            {
                var service = new DynamicServiceService(recipeAction.ExternalService,
                    scope.ServiceProvider.GetService<IRecipeManager>(),
                    scope.ServiceProvider.GetService<IActionDispatcher>(),
                    scope.ServiceProvider.GetService<IExternalServiceManager>());

                return await service.ExecuteWitchcraftToComputeExternalService();
            }
        }
    }
}