using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BtcTransmuter.Abstractions.Actions;
using BtcTransmuter.Abstractions.Recipes;
using BtcTransmuter.Data.Entities;
using BtcTransmuter.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace BtcTransmuter.Tests
{
    public class ActionDispatcherTests
    {
        [Fact]
        public async Task ActionDispatcher_Dispatch_NothingToExecute()
        {
            var recipeManager = new Mock<IRecipeManager>();
            recipeManager.Setup(manager => manager.AddRecipeInvocation(It.IsAny<RecipeInvocation>()));

            var logger = new Mock<ILogger<ActionDispatcher>>();

            //no handlers available
            var actionDispatcher =
                new ActionDispatcher(new List<IActionHandler>(), recipeManager.Object, logger.Object);
            await actionDispatcher.Dispatch(new Dictionary<string,(object data, string json)>(), new RecipeAction());
            recipeManager.Verify(manager => manager.AddRecipeInvocation(It.IsAny<RecipeInvocation>()), Times.Never);


            //handler available but it does not handle the recipe action's request
            var handler = new Mock<IActionHandler>(MockBehavior.Strict);
            handler.Setup(actionHandler => actionHandler.Execute(It.IsAny<Dictionary<string, (object data, string json)>>(), It.IsAny<RecipeAction>()))
                .ReturnsAsync(
                    () => new ActionHandlerResult()
                    {
                        Executed = false
                    });
            
            handler.Setup(actionHandler => actionHandler.CanExecute(It.IsAny<Dictionary<string, (object data, string json)>>(), It.IsAny<RecipeAction>()))
                .ReturnsAsync(false);
            actionDispatcher = new ActionDispatcher(new List<IActionHandler>() {handler.Object}, recipeManager.Object,
                logger.Object);
            await actionDispatcher.Dispatch( new Dictionary<string,(object data, string json)>(), new RecipeAction());

            recipeManager.Verify(manager => manager.AddRecipeInvocation(It.IsAny<RecipeInvocation>()), Times.Never);
        }

        [Fact]
        public async Task ActionDispatcher_Dispatch_ExecuteAndLog()
        {
            var recipeManager = new Mock<IRecipeManager>();
            recipeManager.Setup(manager => manager.AddRecipeInvocation(It.IsAny<RecipeInvocation>()))
                .Returns(Task.CompletedTask);

            var logger = new Mock<ILogger<ActionDispatcher>>();

            //handler available and executes correctly
            var handler = new Mock<IActionHandler>(MockBehavior.Strict);
            handler.Setup(actionHandler => actionHandler.CanExecute(It.IsAny<Dictionary<string, (object data, string json)>>(), It.IsAny<RecipeAction>()))
                .ReturnsAsync(true);
            handler.Setup(actionHandler => actionHandler.Execute(It.IsAny<Dictionary<string, (object data, string json)>>(), It.IsAny<RecipeAction>()))
                .ReturnsAsync(
                    () => new ActionHandlerResult()
                    { 
                        Executed = true,
                        Result = "Something happened"
                    });
            var actionDispatcher = new ActionDispatcher(new List<IActionHandler>() {handler.Object},
                recipeManager.Object,
                logger.Object);
            await actionDispatcher.Dispatch(new Dictionary<string,(object data, string json)>(), new RecipeAction());

            recipeManager.Verify(
                manager => manager.AddRecipeInvocation(It.Is<RecipeInvocation>(invocation =>
                    invocation.ActionResult == "Something happened")), Times.Once);
        }
        
        [Fact]
        public async Task ActionDispatcher_Dispatch_ExecuteAndLogEvenWhenException()
        {
            var recipeManager = new Mock<IRecipeManager>();
            recipeManager.Setup(manager => manager.AddRecipeInvocation(It.IsAny<RecipeInvocation>()))
                .Returns(Task.CompletedTask);

            var logger = new Mock<ILogger<ActionDispatcher>>();

            //handler available but it throws an uncaight exception
            var handler = new Mock<IActionHandler>(MockBehavior.Strict);
            handler.Setup(actionHandler => actionHandler.CanExecute(It.IsAny<Dictionary<string, (object data, string json)>>(), It.IsAny<RecipeAction>()))
                .ReturnsAsync(true);
            handler.Setup(actionHandler => actionHandler.Execute(It.IsAny<Dictionary<string, (object data, string json)>>(), It.IsAny<RecipeAction>()))
                .Throws(new Exception("Something happened"));
            var actionDispatcher = new ActionDispatcher(new List<IActionHandler>() {handler.Object},
                recipeManager.Object,
                logger.Object);
            await actionDispatcher.Dispatch( new Dictionary<string,(object data, string json)>(), new RecipeAction());

            recipeManager.Verify(
                manager => manager.AddRecipeInvocation(It.Is<RecipeInvocation>(invocation =>
                    invocation.ActionResult == "Something happened")), Times.Once);
        }
        
        [Fact]
        public async Task ActionDispatcher_Dispatch_ExecuteAndLogMultipleHandlers()
        {
            var recipeManager = new Mock<IRecipeManager>();
            recipeManager.Setup(manager => manager.AddRecipeInvocation(It.IsAny<RecipeInvocation>()))
                .Returns(Task.CompletedTask);

            var logger = new Mock<ILogger<ActionDispatcher>>();

            //multiple handlers can handle the same action, interesting times ahead!
            var handler = new Mock<IActionHandler>(MockBehavior.Strict);
            handler.Setup(actionHandler => actionHandler.CanExecute(It.IsAny<Dictionary<string, (object data, string json)>>(), It.IsAny<RecipeAction>()))
                .ReturnsAsync(true);
            handler.Setup(actionHandler => actionHandler.Execute(It.IsAny<Dictionary<string, (object data, string json)>>(), It.IsAny<RecipeAction>()))
                .Throws(new Exception("Something happened"));
            var handler2 = new Mock<IActionHandler>(MockBehavior.Strict);
            handler2.Setup(actionHandler => actionHandler.CanExecute(It.IsAny<Dictionary<string, (object data, string json)>>(), It.IsAny<RecipeAction>()))
                .ReturnsAsync(true);
            handler2.Setup(actionHandler => actionHandler.Execute(It.IsAny<Dictionary<string, (object data, string json)>>(), It.IsAny<RecipeAction>()))
                .ReturnsAsync(
                    () => new ActionHandlerResult()
                    { 
                        Executed = true,
                        Result = "Something else happened"
                    });
            var actionDispatcher = new ActionDispatcher(new List<IActionHandler>() {handler.Object, handler2.Object},
                recipeManager.Object,
                logger.Object);
            await actionDispatcher.Dispatch( new Dictionary<string,(object data, string json)>(), new RecipeAction());

            recipeManager.Verify(
                manager => manager.AddRecipeInvocation(It.Is<RecipeInvocation>(invocation =>
                    invocation.ActionResult == "Something happened")), Times.Once);
            recipeManager.Verify(
                manager => manager.AddRecipeInvocation(It.Is<RecipeInvocation>(invocation =>
                    invocation.ActionResult == "Something else happened")), Times.Once);
        }
    }
}