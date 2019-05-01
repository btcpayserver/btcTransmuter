using BtcTransmuter.Extension.Recipe.Actions.ToggleRecipe;
using BtcTransmuter.Tests.Base;

namespace BtcTransmuter.Extension.Recipe.Tests
{
    public class ToggleRecipeActionHandlerTests : BaseActionTest<ToggleRecipeActionHandler, ToggleRecipeData, Data.Entities.Recipe>
    {
        protected override ToggleRecipeActionHandler GetActionHandlerInstance(params object[] setupArgs)
        {
            return new ToggleRecipeActionHandler();
        }
    }
}