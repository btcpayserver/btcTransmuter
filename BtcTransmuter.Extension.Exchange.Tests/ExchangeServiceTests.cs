using System;
using BtcTransmuter.Data.Entities;
using BtcTransmuter.Data.Models;
using System.Linq;
using BtcTransmuter.Extension.Exchange.ExternalServices.Exchange;
using BtcTransmuter.Tests.Base;
using Xunit;
using Assert = Xunit.Assert;

namespace BtcTransmuter.Extension.Exchange.Tests
{
    public class ExchangeServiceTests:BaseExternalServiceTest<ExchangeService,ExchangeExternalServiceData >
    {
        [Fact]
        public void ExchangeService_GetAvailableExchanges()
        {
            Assert.True(ExchangeService.GetAvailableExchanges().Any());
        }

        [Fact]
        public void ExchangeService_CanInitiate()
        {
            //import for service discovery to be able to initiate without constructor
            _ = new ExchangeService();
            
            Assert.Throws<ArgumentException>(() =>
            {
                _ = GetExternalService(new ExternalServiceData()
                {
                    Type = "invalid"
                });
            });
            
           var data = new ExchangeExternalServiceData()
           {
               PublicKey = "test",
               PairedDate = DateTime.Now
           };
           var externalServiceData = new ExternalServiceData()
           {
               Type = ExchangeService.ExchangeServiceType,
               Name = "something"
           };
           externalServiceData.Set(data);
           
           var exchangeService = new ExchangeService(externalServiceData);
           Assert.Equal(exchangeService.GetData().PublicKey, data.PublicKey);
           Assert.Equal(exchangeService.GetData().PairedDate, data.PairedDate);
        }
        
        [Fact]
        public void ExchangeService_CanConstructClient()
        {
            var InvalidData = new ExchangeExternalServiceData()
            {
                PublicKey = "test",
                PairedDate = DateTime.Now,
            };
            var externalServiceData = new ExternalServiceData()
            {
                Type = ExchangeService.ExchangeServiceType,
                Name = "something"
            };
            externalServiceData.Set(InvalidData);
           
            var exchangeService = GetExternalService(externalServiceData);
            Assert.ThrowsAny<Exception>(() => exchangeService.ConstructClient());
            
            
            var validData = new ExchangeExternalServiceData()
            {
                PublicKey = "test",
                PairedDate = DateTime.Now,
                ExchangeName = "Binance",
                PrivateKey = "aa"
            };
            externalServiceData.Set(validData);
            
            Assert.NotNull(exchangeService.ConstructClient());
        }


        protected override ExchangeService GetExternalService(params object[] setupArgs)
        {
            if (setupArgs?.Any()?? false)
            {
                return new ExchangeService((ExternalServiceData) setupArgs.First());
            }
            return new ExchangeService();
        }
    }
}