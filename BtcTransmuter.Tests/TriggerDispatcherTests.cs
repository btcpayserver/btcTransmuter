using System.Collections.Generic;
using System.Threading.Tasks;
using BtcTransmuter.Abstractions.Actions;
using BtcTransmuter.Abstractions.Recipes;
using BtcTransmuter.Abstractions.Triggers;
using BtcTransmuter.Data.Entities;
using BtcTransmuter.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace BtcTransmuter.Tests
{
    public class TriggerDispatcherTests
    {
        [Fact]
        public async Task TriggerDispatcher_Dispatch_NothingToExecute()
        {
            var recipeManager = new Mock<IRecipeManager>();
            recipeManager.Setup(manager => manager.GetRecipes(It.IsAny<RecipesQuery>()))
                .ReturnsAsync(new List<Recipe>()
                {
                    new Recipe()
                    {
                        RecipeTrigger = new RecipeTrigger()
                        {
                            TriggerId = new TestTrigger().Id,
                            DataJson = "sadasd"
                        }
                    }
                });

            var logger = new Mock<ILogger<TriggerDispatcher>>();
            var actionDispatcher = new Mock<IActionDispatcher>();

            //no handlers available
            var triggerDispatcher =
                new TriggerDispatcher(new List<ITriggerHandler>(), recipeManager.Object, actionDispatcher.Object,
                    logger.Object);


            await triggerDispatcher.DispatchTrigger(new TestTrigger()
            {
                DataJson = "{}"
            });

            actionDispatcher.Verify(dispatcher => dispatcher.Dispatch(It.IsAny<object>(), It.IsAny<RecipeAction>()),
                Times.Never);
            
        }

        private class TestTrigger : ITrigger
        {
            public string DataJson { get; set; }
            public string Id => "Test";
        }
    }
}