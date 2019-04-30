using System;
using System.Linq;
using BtcTransmuter.Abstractions.Actions;
using BtcTransmuter.Abstractions.ExternalServices;
using BtcTransmuter.Abstractions.Recipes;
using BtcTransmuter.Data.Entities;
using BtcTransmuter.Extension.BtcPayServer.ExternalServices.BtcPayServer;
using BtcTransmuter.Extension.DynamicService.ExternalServices.DynamicService;
using BtcTransmuter.Tests.Base;
using Xunit;

namespace BtcTransmuter.Extension.DynamicServices.Tests
{
    public class
        DynamicServiceServiceTests : BaseExternalServiceTest<DynamicServiceService, DynamicServiceExternalServiceData>
    {
        
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