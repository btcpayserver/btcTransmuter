using System.Collections.Generic;
using BtcTransmuter.Abstractions.ExternalServices;
using BtcTransmuter.Data.Entities;

namespace BtcTransmuter.Tests.Base
{
    public class TestExternalService : BaseExternalService<List<string>>
    {
        public override string ExternalServiceType { get; } = "test";
        public override string Name { get; } = "a";
        public override string Description { get; }= "a";
        public override string ViewPartial { get; }= "a";
        public override string ControllerName { get; }= "a";

        public TestExternalService()
        {
             
        }

        public TestExternalService(ExternalServiceData externalServiceData):base(externalServiceData)
        {
             
        }
    }
}