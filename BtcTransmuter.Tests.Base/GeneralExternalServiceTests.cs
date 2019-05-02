using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BtcTransmuter.Data.Entities;
using Xunit;

namespace BtcTransmuter.Tests.Base
{
    public class GeneralExternalServiceTests:BaseExternalServiceTest<TestExternalService, List<string>>{
        protected override TestExternalService GetExternalService(params object[] setupArgs)
        {
            if (setupArgs?.Any()?? false)
            {
                return new TestExternalService((ExternalServiceData) setupArgs.First());
            }
            return new TestExternalService();
        }
         
        [Fact]
        public void OnlyInitWithCorrectData()
        {
            Assert.Throws<ArgumentException>(() => new TestExternalService(new ExternalServiceData()
            {
                Type = "wrong"
            }));
        }
    }
}