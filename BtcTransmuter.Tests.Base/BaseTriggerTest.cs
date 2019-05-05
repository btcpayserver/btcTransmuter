using System;
using System.Threading.Tasks;
using BtcTransmuter.Abstractions.Triggers;
using BtcTransmuter.Data.Entities;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace BtcTransmuter.Tests.Base
{
    public abstract class BaseTriggerTest<TTriggerHandler, TTriggerData, TTriggerParameters> : BaseTests
        where TTriggerHandler : BaseTriggerHandler<TTriggerData, TTriggerParameters> where TTriggerParameters : class
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


        [Fact]
        public async Task EditData_GenerateActionResultToEditRecipeAction()
        {
            ConfigureDependencyHelper();
            var triggerHandler = GetTriggerHandlerInstance();

            var newRecipeTrigger = new RecipeTrigger()
            {
                TriggerId = triggerHandler.TriggerId,
            };
            var newResult = Assert.IsType<RedirectToActionResult>(await triggerHandler.EditData(newRecipeTrigger));
            Assert.Equal(triggerHandler.ControllerName, newResult.ControllerName);

            var existingrecipeTrigger = new RecipeTrigger()
            {
                TriggerId = triggerHandler.TriggerId,
                Id = Guid.NewGuid().ToString()
            };
            var existingResult =
                Assert.IsType<RedirectToActionResult>(await triggerHandler.EditData(existingrecipeTrigger));
            Assert.Equal(triggerHandler.ControllerName, existingResult.ControllerName);
            Assert.NotEqual(newResult.RouteValues["identifier"], existingResult.RouteValues["identifier"]);
        }


        protected virtual TTriggerHandler GetTriggerHandlerInstance(params object[] setupArgs)
        {
            return Activator.CreateInstance<TTriggerHandler>();
        }
    }
}