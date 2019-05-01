using System;
using BtcTransmuter.Extension.Email.Actions.SendEmail;
using BtcTransmuter.Tests.Base;

namespace BtcTransmuter.Extension.Email.Tests
{
    public class SendEmailDataActionHandlerTests : BaseActionTest<SendEmailDataActionHandler, SendEmailData, string>
    {
        protected override SendEmailDataActionHandler GetActionHandlerInstance(params object[] setupArgs)
        {
           return new SendEmailDataActionHandler();
        }
    }
}