using System;
using System.Threading.Tasks;
using BtcTransmuter.Data.Entities;
using Xunit;

namespace BtcTransmuter.Tests.Base
{
    public class GeneralExternalServiceTests:BaseExternalServiceTest<TestExternalService, string>{
        protected override TestExternalService GetExternalService(params object[] setupArgs)
        {
            return new TestExternalService();
        }
         
        [Fact]
        public async Task OnlyInitWithCorrectData()
        {
            Assert.Throws<ArgumentException>(() => new TestExternalService(new ExternalServiceData()
            {
                Type = "wrong"
            }));
        }
    }
}