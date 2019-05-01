using System;
using System.Linq;
using System.Threading.Tasks;
using BtcTransmuter.Abstractions.Actions;
using BtcTransmuter.Abstractions.ExternalServices;
using BtcTransmuter.Abstractions.Helpers;
using BtcTransmuter.Abstractions.Recipes;
using BtcTransmuter.Data.Entities;
using BtcTransmuter.Extension.BtcPayServer.ExternalServices.BtcPayServer;
using BtcTransmuter.Extension.DynamicService.ExternalServices.DynamicService;
using BtcTransmuter.Extension.Lightning.ExternalServices.NBXplorerWallet;
using BtcTransmuter.Extension.NBXplorer.Services;
using BtcTransmuter.Tests.Base;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace BtcTransmuter.Extension.DynamicServices.Tests
{
    public class
        DynamicServiceServiceTests : BaseExternalServiceTest<DynamicServiceService, DynamicServiceExternalServiceData>
    {
        public override Task CanSerializeData()
        {
            ConfigureDependencyHelper();
            using (var scope = DependencyHelper.ServiceScopeFactory.CreateScope())
            {
                var x = GetExternalService();
                var instance = new DynamicServiceExternalServiceData();
                var externalService = GetExternalService(new ExternalServiceData()
                    {
                        Type = x.ExternalServiceType
                    },
                    scope.ServiceProvider.GetRequiredService<IRecipeManager>(),
                    scope.ServiceProvider.GetRequiredService<IActionDispatcher>(),
                    scope.ServiceProvider.GetRequiredService<IExternalServiceManager>()

                );
                externalService.SetData(instance);
                externalService.GetData();
            }
            return Task.CompletedTask;
        }

        protected override DynamicServiceService GetExternalService(params object[] setupArgs)
        {
            if (setupArgs?.Any()?? false)
            {
                
                return new DynamicServiceService(
                    (ExternalServiceData) setupArgs.First(o => o is ExternalServiceData), 
                    (IRecipeManager) setupArgs.First(o => o is IRecipeManager),
                    (IActionDispatcher) setupArgs.First(o => o is IActionDispatcher),
                    (IExternalServiceManager) setupArgs.First(o => o is IExternalServiceManager));
            }
            return new DynamicServiceService();
        }
    }
}