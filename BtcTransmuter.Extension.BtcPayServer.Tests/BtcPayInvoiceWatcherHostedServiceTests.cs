using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using BtcTransmuter.Abstractions.ExternalServices;
using BtcTransmuter.Abstractions.Triggers;
using BtcTransmuter.Data.Entities;
using BtcTransmuter.Data.Models;
using BtcTransmuter.Extension.BtcPayServer.ExternalServices.BtcPayServer;
using BtcTransmuter.Extension.BtcPayServer.HostedServices;
using BtcTransmuter.Extension.BtcPayServer.Triggers.InvoiceStatusChanged;
using BtcTransmuter.Tests.Base;
using Moq;
using NBitcoin;
using NBitpayClient;
using Newtonsoft.Json;
using Xunit;
using Assert = Xunit.Assert;

namespace BtcTransmuter.Extension.BtcPayServer.Tests
{
    public class BtcPayInvoiceWatcherHostedServiceTests : BaseTests
    {
        [Fact]
        public async Task CanDetectInvoiceChanges()
        {
            var externalServiceManager = new Mock<IExternalServiceManager>();
            var triggerDispatcher = new Mock<ITriggerDispatcher>();

            var externalServices = new List<ExternalServiceData>()
            {
                new ExternalServiceData()
                {
                    Type = BtcPayServerService.BtcPayServerServiceType,
                    Id = "A",
                    Name = "A",
                    DataJson = JsonConvert.SerializeObject(new BtcPayServerExternalServiceData())
                }
            };
            var invoices = new List<BtcPayInvoice>()
            {
                new BtcPayInvoice()
                {
                    Id = "A-A",
                    Status = Invoice.STATUS_NEW,
                    CurrentTime = DateTimeOffset.Now,
                    InvoiceTime = DateTimeOffset.Now,
                    ExpirationTime = DateTimeOffset.Now,
                    
                }
            };
            ITrigger dispatchedTrigger = null;

            triggerDispatcher.Setup(dispatcher => dispatcher.DispatchTrigger(It.IsAny<ITrigger>()))
                .Callback<ITrigger>((x) => { dispatchedTrigger = x; }).Returns(Task.CompletedTask);


            var mockBitPay = new TestBitpay();
            mockBitPay.Invoices = () => invoices.ToArray();

            var btcpayserviceMock = new Mock<BtcPayServerService>();
            btcpayserviceMock.Setup(serverService => serverService.ConstructClient())
                .Returns(() => mockBitPay);

            btcpayserviceMock.Setup(serverService => serverService.CheckAccess()).ReturnsAsync(() => true);
            btcpayserviceMock.Setup(serverService => serverService.GetData()).Returns(() => externalServices[0].Get<BtcPayServerExternalServiceData>());

            externalServiceManager
                .Setup(manager => manager.GetExternalServicesData(It.IsAny<ExternalServicesDataQuery>()))
                .ReturnsAsync(() => externalServices);

            var flag = 0;
            externalServiceManager
                .Setup(manager => manager.UpdateInternalData(It.IsAny<string>(), It.IsAny<object>()))
                .Callback<string, object>((key, data) =>
                {
                    externalServices[0].Set(data);
                    flag++;
                    externalServiceManager.Raise(manager => manager.ExternalServiceDataUpdated+=null, new UpdatedItem<ExternalServiceData>()
                    {
                        Item = externalServices[0],
                        Action = UpdatedItem<ExternalServiceData>.UpdateAction.Updated
                    });
                })
                .Returns(Task.CompletedTask);

            
            var service =
                new TestBtcPayInvoiceWatcherHostedService(externalServiceManager.Object, triggerDispatcher.Object);

            service.ConvertData = data => btcpayserviceMock.Object;

            await service.StartAsync(CancellationToken.None);

            while (flag == 0)
            {
                await Task.Delay(200);
            }
            externalServiceManager.Verify(
                manager => manager.GetExternalServicesData(It.IsAny<ExternalServicesDataQuery>()), Times.Once);

            triggerDispatcher.Verify(dispatcher => dispatcher.DispatchTrigger(It.IsAny<ITrigger>()), Times.Once);
            var invoice = Assert.IsType<InvoiceStatusChangedTrigger>(dispatchedTrigger).Data.Invoice;
            Assert.Equal(invoices[0].Id, invoice.Id);
            Assert.Equal(invoices[0].Status, invoice.Status);
            invoices[0].Status = Invoice.STATUS_PAID;
            while (flag == 1)
            {
                await Task.Delay(200);
            }
           

            triggerDispatcher.Verify(dispatcher => dispatcher.DispatchTrigger(It.IsAny<ITrigger>()), Times.Exactly(2));

            invoice = Assert.IsType<InvoiceStatusChangedTrigger>(dispatchedTrigger).Data.Invoice;
            Assert.Equal(invoices[0].Id, invoice.Id);
            Assert.Equal(invoices[0].Status, invoice.Status);
        }

        public class TestBtcPayInvoiceWatcherHostedService : BtcPayInvoiceWatcherHostedService
        {
            public TestBtcPayInvoiceWatcherHostedService(IExternalServiceManager externalServiceManager,
                ITriggerDispatcher triggerDispatcher) : base(externalServiceManager, triggerDispatcher)
            {
            }

            public Func<ExternalServiceData, BtcPayServerService> ConvertData = null;

            protected override BtcPayServerService GetServiceFromData(ExternalServiceData externalServiceData)
            {
                return ConvertData == null
                    ? base.GetServiceFromData(externalServiceData)
                    : ConvertData(externalServiceData);
            }

            protected override TimeSpan CheckInterval { get; } = TimeSpan.FromSeconds(2);
        }


        public class TestBitpay : Bitpay
        {
            public TestBitpay(Key ecKey, Uri envUrl) : base(ecKey, envUrl)
            {
            }

            public TestBitpay(): base(new Key(), new Uri("https://gozo.com"))
            {
                
            }

            public Func<Invoice[]> Invoices;


            public override Task<T[]> GetInvoicesAsync<T>(DateTime? dateStart = null, DateTime? dateEnd = null)
            {
                if (Invoices != null)
                {
                    return Task.FromResult(Invoices.Invoke().Select(invoice => (T) invoice).ToArray());
                }
                return base.GetInvoicesAsync<T>(dateStart, dateEnd);
            }
        }
    }
}