using System;
using BtcTransmuter.Data.Entities;
using BtcTransmuter.Data.Models;
using BtcTransmuter.Extension.Exchange.ExternalServices.Exchange;
using Microsoft.EntityFrameworkCore.Internal;
using Xunit;
using Assert = Xunit.Assert;

namespace BtcTransmuter.Extension.Exchange.Tests
{
    public class ExchangeServiceTests
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
                _ = new ExchangeService(new ExternalServiceData()
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
           
            var exchangeService = new ExchangeService(externalServiceData);
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
        
        
    }
}