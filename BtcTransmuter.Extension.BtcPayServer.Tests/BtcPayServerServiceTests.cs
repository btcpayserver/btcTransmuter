using System;
using System.Linq;
using BtcTransmuter.Data.Entities;
using BtcTransmuter.Extension.BtcPayServer.ExternalServices.BtcPayServer;
using BtcTransmuter.Tests.Base;
using Xunit;

namespace BtcTransmuter.Extension.BtcPayServer.Tests
{
	public class
		BtcPayServerServiceTests : BaseExternalServiceTest<BtcPayServerService, BtcPayServerExternalServiceData>
	{
		protected override BtcPayServerService GetExternalService(params object[] setupArgs)
		{
			if (setupArgs?.Any() ?? false)
			{
				return new BtcPayServerService((ExternalServiceData) setupArgs.First());
			}

			return new BtcPayServerService();
		}
	}
}