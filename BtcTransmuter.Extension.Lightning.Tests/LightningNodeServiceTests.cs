using System;
using System.Linq;
using System.Threading.Tasks;
using BtcTransmuter.Abstractions.Helpers;
using BtcTransmuter.Data.Entities;
using BtcTransmuter.Extension.Lightning.ExternalServices.LightningNode;
using BtcTransmuter.Extension.Lightning.ExternalServices.NBXplorerWallet;
using BtcTransmuter.Extension.NBXplorer.Services;
using BtcTransmuter.Tests.Base;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace BtcTransmuter.Extension.Lightning.Tests
{


    public class
        LightningNodeServiceTests : BaseExternalServiceTest<LightningNodeService, LightningNodeExternalServiceData>
    {
        public override Task CanSerializeData()
        {
            ConfigureDependencyHelper();
            using (var scope = DependencyHelper.ServiceScopeFactory.CreateScope())
            {


                var x = GetExternalService();
                var instance = new LightningNodeExternalServiceData();
                var externalService = GetExternalService(new ExternalServiceData()
                    {
                        Type = x.ExternalServiceType
                    },
                    scope.ServiceProvider.GetRequiredService<SocketFactory>(),
                    scope.ServiceProvider.GetRequiredService<NBXplorerSummaryProvider>(),
                    scope.ServiceProvider.GetRequiredService<NBXplorerClientProvider>()

                );
                externalService.SetData(instance);
                externalService.GetData();
            }

            return Task.CompletedTask;
        }



        protected override LightningNodeService GetExternalService(params object[] setupArgs)
        {
            if (setupArgs.Any())
            {
                return new LightningNodeService(
                    (ExternalServiceData) setupArgs.Single(o => o is ExternalServiceData),
                    (NBXplorerClientProvider) setupArgs.Single(o => o is NBXplorerClientProvider),
                    (NBXplorerSummaryProvider) setupArgs.Single(o => o is NBXplorerSummaryProvider),
                    (SocketFactory) setupArgs.Single(o => o is SocketFactory));
            }

            return new LightningNodeService();
        }
    }
}