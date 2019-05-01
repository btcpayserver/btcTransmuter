using System;
using BtcTransmuter.Extension.Timer.Triggers.Timer;
using BtcTransmuter.Tests.Base;
using Xunit;

namespace BtcTransmuter.Extension.Timer.Tests
{
    public class TimerTriggerHandlerTests : BaseTriggerTest<TimerTriggerHandler, TimerTriggerData, TimerTriggerParameters>
    {
        protected override TimerTriggerHandler GetTriggerHandlerInstance(params object[] setupArgs)
        {
            return new TimerTriggerHandler();
        }
    }
}