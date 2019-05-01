using System;
using BtcTransmuter.Extension.Recipe.Actions.CreateRecipe;
using BtcTransmuter.Tests.Base;
using Xunit;

namespace BtcTransmuter.Extension.Recipe.Tests
{
    public class CreateRecipeActionHandlerTests : BaseActionTest<CreateRecipeActionHandler, CreateRecipeData, Data.Entities.Recipe>
    {
        protected override CreateRecipeActionHandler GetActionHandlerInstance(params object[] setupArgs)
        {
            return new CreateRecipeActionHandler();
        }
    }
}