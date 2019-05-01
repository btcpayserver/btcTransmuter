using System;
using BtcTransmuter.Extension.Operators.Actions.ChooseNumericValue;
using BtcTransmuter.Tests.Base;
using Xunit;

namespace BtcTransmuter.Extension.Operators.Tests
{
    public class ChooseNumericValueDataActionHandlerTests : BaseActionTest<ChooseNumericValueDataActionHandler,ChooseNumericValueData, string>
    {
        protected override ChooseNumericValueDataActionHandler GetActionHandlerInstance(params object[] setupArgs)
        {
            return new ChooseNumericValueDataActionHandler();
        }
    }
}