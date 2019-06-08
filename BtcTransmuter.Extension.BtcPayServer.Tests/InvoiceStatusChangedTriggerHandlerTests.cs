using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BtcTransmuter.Data.Entities;
using BtcTransmuter.Extension.BtcPayServer.HostedServices;
using BtcTransmuter.Extension.BtcPayServer.Triggers.InvoiceStatusChanged;
using BtcTransmuter.Tests.Base;
using Newtonsoft.Json;
using Xunit;
using Assert = Xunit.Assert;

namespace BtcTransmuter.Extension.BtcPayServer.Tests
{
    public class
        InvoiceStatusChangedTriggerHandlerTests : BaseTriggerTest<InvoiceStatusChangedTriggerHandler,
            InvoiceStatusChangedTriggerData,
            InvoiceStatusChangedTriggerParameters>
    {
        [Fact]
        public async Task IsTriggered_ExecutesCorrectly()
        {
            var handler = GetTriggerHandlerInstance();

            var parameters = new InvoiceStatusChangedTriggerParameters()
            {
                Status = null
            };
            Assert.True(await handler.IsTriggered(new InvoiceStatusChangedTrigger()
                {
                    Data = new InvoiceStatusChangedTriggerData()
                    {
                        ExternalServiceId = "A",
                        Invoice = new BtcPayInvoice()
                        {
                            Status = BtcPayInvoice.STATUS_NEW,
                            CurrentTime = DateTimeOffset.Now,
                            InvoiceTime = DateTimeOffset.Now,
                            ExpirationTime = DateTimeOffset.Now
                        }
                    }
                },
                new RecipeTrigger()
                {
                    TriggerId = handler.TriggerId,
                    ExternalServiceId = "A",
                    DataJson = JsonConvert.SerializeObject(parameters)
                }));

			//the external service id is not the same, if it triggers you are in deep shit
            Assert.False(await handler.IsTriggered(new InvoiceStatusChangedTrigger()
	            {
		            Data = new InvoiceStatusChangedTriggerData()
		            {
			            ExternalServiceId = "B",
			            Invoice = new BtcPayInvoice()
			            {
				            Status = BtcPayInvoice.STATUS_NEW,
				            CurrentTime = DateTimeOffset.Now,
				            InvoiceTime = DateTimeOffset.Now,
				            ExpirationTime = DateTimeOffset.Now
			            }
		            }
	            },
	            new RecipeTrigger()
	            {
		            TriggerId = handler.TriggerId,
		            ExternalServiceId = "A",
		            DataJson = JsonConvert.SerializeObject(parameters)
	            }));

			Assert.True(await handler.IsTriggered(new InvoiceStatusChangedTrigger()
                {
                    Data = new InvoiceStatusChangedTriggerData()
                    {
                        ExternalServiceId = "A",
                        Invoice = new BtcPayInvoice()
                        {
                            Status = BtcPayInvoice.STATUS_PAID,
                            CurrentTime = DateTimeOffset.Now,
                            InvoiceTime = DateTimeOffset.Now,
                            ExpirationTime = DateTimeOffset.Now
                        }
                    }
                },
                new RecipeTrigger()
                {
                    TriggerId = handler.TriggerId,
                    ExternalServiceId = "A",
                    DataJson = JsonConvert.SerializeObject(parameters)
                }));

            parameters.Status = new List<string>()
            {
	            BtcPayInvoice.STATUS_PAID
			};

            Assert.True(await handler.IsTriggered(new InvoiceStatusChangedTrigger()
                {
                    Data = new InvoiceStatusChangedTriggerData()
                    {
                        ExternalServiceId = "A",
                        Invoice = new BtcPayInvoice()
                        {
                            Status = BtcPayInvoice.STATUS_PAID,
                            CurrentTime = DateTimeOffset.Now,
                            InvoiceTime = DateTimeOffset.Now,
                            ExpirationTime = DateTimeOffset.Now
                        }
                    }
                },
                new RecipeTrigger()
                {
                    TriggerId = handler.TriggerId,
                    ExternalServiceId = "A",
                    DataJson = JsonConvert.SerializeObject(parameters)
                }));


            Assert.False(await handler.IsTriggered(new InvoiceStatusChangedTrigger()
                {
                    Data = new InvoiceStatusChangedTriggerData()
                    {
                        ExternalServiceId = "A",
                        Invoice = new BtcPayInvoice()
                        {
                            Status = BtcPayInvoice.STATUS_NEW,
                            CurrentTime = DateTimeOffset.Now,
                            InvoiceTime = DateTimeOffset.Now,
                            ExpirationTime = DateTimeOffset.Now
                        }
                    }
                },
                new RecipeTrigger()
                {
                    TriggerId = handler.TriggerId,
                    ExternalServiceId = "A",
                    DataJson = JsonConvert.SerializeObject(parameters)
                }));
        }
    }
}