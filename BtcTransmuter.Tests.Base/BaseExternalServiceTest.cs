using System;
using System.Threading.Tasks;
using BtcTransmuter.Abstractions.ExternalServices;
using Xunit;

namespace BtcTransmuter.Tests.Base
{
    public abstract class BaseExternalServiceTest<TExternalService, TExternalServiceData> : BaseTests
        where TExternalService : BaseExternalService<TExternalServiceData>
    {
        [Fact]
        public void BasicActionPropertiesSet()
        {
            var externalService = GetExternalService();
            Assert.NotNullOrEmpty(externalService.Name);
            Assert.NotNullOrEmpty(externalService.ExternalServiceType);
            Assert.NotNullOrEmpty(externalService.ViewPartial);
            Assert.NotNullOrEmpty(externalService.ControllerName);
            Assert.NotNullOrEmpty(externalService.Description);
        }

        [Fact]
        public async Task CanSerializeData()
        {
            var instance = Activator.CreateInstance<TExternalServiceData>();
            var externalService = GetExternalService();
            externalService.SetData(instance);
            externalService.GetData();
        }
        
        


        protected abstract TExternalService GetExternalService(params object[] setupArgs);

        
    }
}