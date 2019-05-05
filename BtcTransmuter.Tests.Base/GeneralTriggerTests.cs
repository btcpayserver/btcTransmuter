using System;
using System.Threading.Tasks;
using BtcTransmuter.Abstractions.ExternalServices;
using BtcTransmuter.Abstractions.Triggers;
using BtcTransmuter.Data.Entities;
using Newtonsoft.Json;
using Xunit;

namespace BtcTransmuter.Tests.Base
{
    public class GeneralTriggerTests : BaseTriggerTest<GeneralTriggerTests.TestBaseTriggerHandler,
        GeneralTriggerTests.TestTriggerData, GeneralTriggerTests.TestTriggerParameters>
    {

        [Fact]
        public  async Task IsTriggered_TriggersBasicsCorrectly()
        {
            var handler = GetTriggerHandlerInstance();
            Assert.False(await handler.IsTriggered(new TestTrigger(), new RecipeTrigger()
            {
                TriggerId = new Guid().ToString()
            }));
            
            Assert.False(await handler.IsTriggered(new TestTrigger()
            {
                Id = "Wrong"
            }, new RecipeTrigger()
            {
                TriggerId = "Wrong"
            }));
            
            Assert.False(await handler.IsTriggered(new TestTrigger()
            {
                Id = "TestTrigger",
                DataJson = JsonConvert.SerializeObject(new TestTriggerData()
                {
                    ExternalServiceId = "Wrong"
                })
            }, new RecipeTrigger()
            {
                TriggerId = "TestTrigger",
                ExternalServiceId = "ExternalServiceId"
            }));
            
            Assert.True(await handler.IsTriggered(new TestTrigger()
            {
                Id = "TestTrigger",
                DataJson = JsonConvert.SerializeObject(new TestTriggerData()
                {
                    ExternalServiceId = "ExternalServiceId"
                })
            }, new RecipeTrigger()
            {
                TriggerId = "TestTrigger",
                ExternalServiceId = "ExternalServiceId"
            }));
        }
        
        public class TestTriggerData: IUseExternalService
        {
            public string ExternalServiceId { get; set; }
        }

        public class TestTrigger : ITrigger
        {
            public string DataJson { get; set; }
            public string Id { get; set; } = "TestTrigger";
        }

        public class TestTriggerParameters: IUseExternalService
        {
            public string ExternalServiceId { get; set; }
        }

        public class TestBaseTriggerHandler : BaseTriggerHandler<TestTriggerData, TestTriggerParameters>
        {
            public override string TriggerId { get; } = "TestTrigger";
            public override string Name { get; } = "TestTrigger";
            public override string Description { get; } = "TestTrigger";
            public override string ViewPartial { get; } = "TestTrigger";
            public override string ControllerName { get; } = "TestTrigger";

            protected override Task<bool> IsTriggered(ITrigger trigger, RecipeTrigger recipeTrigger,
                TestTriggerData triggerData,
                TestTriggerParameters parameters)
            {
                return Task.FromResult(true);
            }
        }
    }
}