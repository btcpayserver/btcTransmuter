using BtcTransmuter.Abstractions.Triggers;
using Xunit;

namespace BtcTransmuter.Tests.Base
{
    public abstract class BaseTriggerTest<TTriggerHandler,TTriggerData, TTriggerParameters>
        where TTriggerHandler: BaseTriggerHandler<TTriggerData, TTriggerParameters> where TTriggerParameters : class
    {

        [Fact]
        public void BasicActionPropertiesSet()
        {
            var actionHandler = GetTriggerHandlerInstance();
            Assert.NotNullOrEmpty(actionHandler.Name);
            Assert.NotNullOrEmpty(actionHandler.TriggerId);
            Assert.NotNullOrEmpty(actionHandler.ViewPartial);
            Assert.NotNullOrEmpty(actionHandler.ViewPartial);
            Assert.NotNullOrEmpty(actionHandler.Description);
        }

        protected abstract TTriggerHandler GetTriggerHandlerInstance(params object[] setupArgs);

    }
}