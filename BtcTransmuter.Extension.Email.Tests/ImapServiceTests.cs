using System.Linq;
using BtcTransmuter.Data.Entities;
using BtcTransmuter.Extension.Email.ExternalServices.Imap;
using BtcTransmuter.Tests.Base;
using Xunit;

namespace BtcTransmuter.Extension.Email.Tests
{
    public class ImapServiceTests: BaseExternalServiceTest<ImapService, ImapExternalServiceData>{
        
        protected override ImapService GetExternalService(params object[] setupArgs)
        {
            if (setupArgs?.Any() ?? false)
            {
                return new ImapService((ExternalServiceData) setupArgs.First());
            }
            return new ImapService();
        }
    }
}