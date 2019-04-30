using System.Linq;
using BtcTransmuter.Data.Entities;
using BtcTransmuter.Extension.Email.ExternalServices.Pop3;
using BtcTransmuter.Tests.Base;

namespace BtcTransmuter.Extension.Email.Tests
{
    public class Pop3ServiceTests: BaseExternalServiceTest<Pop3Service, Pop3ExternalServiceData>{
        
        protected override Pop3Service GetExternalService(params object[] setupArgs)
        {
            if (setupArgs?.Any() ?? false)
            {
                return new Pop3Service((ExternalServiceData) setupArgs.First());
            }
            return new Pop3Service();
        }
    }
}