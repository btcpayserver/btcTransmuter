using BtcTransmuter.Extension.Operators.Actions.Condition;
using BtcTransmuter.Tests.Base;

namespace BtcTransmuter.Extension.Operators.Tests
{
    public class ConditionDataActionHandlerTests : BaseActionTest<ConditionDataActionHandler, ConditionData, string>
    {
        protected override ConditionDataActionHandler GetActionHandlerInstance(params object[] setupArgs)
        {
            return new ConditionDataActionHandler();
        }
    }
}