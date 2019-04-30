using System.Linq;
using BtcTransmuter.Data.Entities;
using BtcTransmuter.Extension.Email.ExternalServices.Smtp;
using BtcTransmuter.Tests.Base;

namespace BtcTransmuter.Extension.Email.Tests
{
    public class SmtpServiceTests: BaseExternalServiceTest<SmtpService, SmtpExternalServiceData>{
        
        protected override SmtpService GetExternalService(params object[] setupArgs)
        {
            if (setupArgs?.Any() ?? false)
            {
                return new SmtpService((ExternalServiceData) setupArgs.First());
            }
            return new SmtpService();
        }
    }
}