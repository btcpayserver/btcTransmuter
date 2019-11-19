using System;
using System.Linq;
using System.Threading.Tasks;
using BtcTransmuter.Abstractions.Helpers;
using BtcTransmuter.Data.Entities;
using BtcTransmuter.Extension.Lightning.ExternalServices.NBXplorerWallet;
using BtcTransmuter.Extension.NBXplorer.Services;
using BtcTransmuter.Tests.Base;
using Microsoft.Extensions.DependencyInjection;

namespace BtcTransmuter.Extension.NBXplorer.Tests
{
    public class NBXplorerWalletServiceTests : BaseExternalServiceTest<NBXplorerWalletService, NBXplorerWalletExternalServiceData>
    {
        public override Task CanSerializeData()
        {
            ConfigureDependencyHelper();
            using (var scope = DependencyHelper.ServiceScopeFactory.CreateScope())
            {


                var x = GetExternalService();
                var instance = new NBXplorerWalletExternalServiceData();
                var externalService = GetExternalService(new ExternalServiceData()
                    {
                        Type = x.ExternalServiceType
                    },
                    scope.ServiceProvider.GetRequiredService<NBXplorerPublicWalletProvider>(),
                    scope.ServiceProvider.GetRequiredService<DerivationSchemeParser>(),
                    scope.ServiceProvider.GetRequiredService<NBXplorerClientProvider>()

                );
                externalService.SetData(instance);
                externalService.GetData();
            }
            return Task.CompletedTask;
        }
        
        

        protected override NBXplorerWalletService GetExternalService(params object[] setupArgs)
        {
            if (setupArgs.Any())
            {
                return new NBXplorerWalletService(
                    (ExternalServiceData) setupArgs.Single(o => o is ExternalServiceData),
                    (NBXplorerPublicWalletProvider) setupArgs.Single(o => o is NBXplorerPublicWalletProvider),
                    (DerivationSchemeParser) setupArgs.Single(o => o is DerivationSchemeParser),
                    (NBXplorerClientProvider) setupArgs.Single(o => o is NBXplorerClientProvider));
            }

            return new NBXplorerWalletService();
        }
    }
}