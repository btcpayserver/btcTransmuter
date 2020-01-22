using System;
using System.Collections.Generic;
using System.Threading;
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
                            DataJson = "{}"
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

            actionDispatcher.Verify(
                dispatcher => dispatcher.Dispatch((Dictionary<string, (object data, string json)>) It.IsAny<object>(),
                    It.IsAny<RecipeAction>()),
                Times.Never);
        }

        [Fact]
        public async Task TriggerDispatcher_Dispatch_ExecuteActionDispatcher()
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
                            DataJson = "{}"
                        },
                        RecipeActions = new List<RecipeAction>()
                        {
                            new RecipeAction()
                        }
                    }
                });

            var logger = new Mock<ILogger<TriggerDispatcher>>();
            var actionDispatcher = new Mock<IActionDispatcher>();

            var triggerHandler = new Mock<ITriggerHandler>();
            triggerHandler.Setup(handler => handler.GetData(It.IsAny<ITrigger>())).ReturnsAsync("hohoho");
            triggerHandler.Setup(handler => handler.IsTriggered(It.IsAny<ITrigger>(), It.IsAny<RecipeTrigger>()))
                .ReturnsAsync(true);

            var triggerHandler2 = new Mock<ITriggerHandler>();
            triggerHandler2.Setup(handler => handler.GetData(It.IsAny<ITrigger>())).ThrowsAsync(new Exception());
            triggerHandler2.Setup(handler => handler.IsTriggered(It.IsAny<ITrigger>(), It.IsAny<RecipeTrigger>()))
                .ReturnsAsync(true);

            var triggerHandler3 = new Mock<ITriggerHandler>();
            triggerHandler3.Setup(handler => handler.GetData(It.IsAny<ITrigger>())).ReturnsAsync("hohoho");
            triggerHandler3.Setup(handler => handler.IsTriggered(It.IsAny<ITrigger>(), It.IsAny<RecipeTrigger>()))
                .ThrowsAsync(new Exception());

            var triggerDispatcher =
                new TriggerDispatcher(
                    new List<ITriggerHandler>() {triggerHandler.Object, triggerHandler2.Object, triggerHandler3.Object},
                    recipeManager.Object, actionDispatcher.Object,
                    logger.Object);


            await triggerDispatcher.DispatchTrigger(new TestTrigger()
            {
                DataJson = "{}"
            });

            var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            while (!cts.IsCancellationRequested)
            {
                try
                {

                    actionDispatcher.Verify(
                        dispatcher => dispatcher.Dispatch(new Dictionary<string, (object data, string json)>(),
                            It.IsAny<RecipeAction>()),
                        Times.Once);
                    break;

                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }

        }

        private class TestTrigger : ITrigger
        {
            public string DataJson { get; set; }
            public string Id => "Test";
        }
    }
}