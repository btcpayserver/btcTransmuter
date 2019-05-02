using System;
using System.Threading.Tasks;
using BtcTransmuter.Abstractions.ExternalServices;
using BtcTransmuter.Data.Entities;
using Xunit;

namespace BtcTransmuter.Tests.Base
{
    public abstract class BaseExternalServiceTest<TExternalService, TExternalServiceData> : BaseTests
        where TExternalService : BaseExternalService<TExternalServiceData>
    {
        [Fact]
        public virtual  void BasicActionPropertiesSet()
        {
            var externalService = GetExternalService();
            Assert.NotNullOrEmpty(externalService.Name);
            Assert.NotNullOrEmpty(externalService.ExternalServiceType);
            Assert.NotNullOrEmpty(externalService.ViewPartial);
            Assert.NotNullOrEmpty(externalService.ControllerName);
            Assert.NotNullOrEmpty(externalService.Description);
        }

        [Fact]
        public virtual Task CanSerializeData()
        {
            var x = GetExternalService();
            var instance = Activator.CreateInstance<TExternalServiceData>();
            var externalService = GetExternalService(new ExternalServiceData()
            {
                Type = x.ExternalServiceType
            });
            externalService.SetData(instance);
            externalService.GetData();
            return Task.CompletedTask;
        }
        
        protected abstract TExternalService GetExternalService(params object[] setupArgs);

        
    }
}