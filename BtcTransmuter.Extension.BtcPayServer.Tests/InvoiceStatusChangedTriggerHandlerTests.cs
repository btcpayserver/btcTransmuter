using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BtcTransmuter.Data.Entities;
using BtcTransmuter.Extension.BtcPayServer.HostedServices;
using BtcTransmuter.Extension.BtcPayServer.Triggers.InvoiceStatusChanged;
using BtcTransmuter.Tests.Base;
using NBitpayClient;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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

			//any status combination
            var parameters = new InvoiceStatusChangedTriggerParameters()
            {
				Conditions = InvoiceStatusChangedController.AllowedStatuses.Select(item => new InvoiceStatusChangeCondition()
				{
					Status = item.Value,
					ExceptionStatuses = InvoiceStatusChangedController.AllowedExceptionStatus.Select(item2 => item2.Value).ToList()
				}).ToList()
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

            Assert.True(await handler.IsTriggered(new InvoiceStatusChangedTrigger()
	            {
		            Data = new InvoiceStatusChangedTriggerData()
		            {
			            ExternalServiceId = "A",
			            Invoice = new BtcPayInvoice()
			            {
				            Status = BtcPayInvoice.STATUS_INVALID,
							ExceptionStatus = new JValue(Invoice.EXSTATUS_PAID_PARTIAL),
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

			//only paid status with no exception statuses
			parameters.Conditions = new List<InvoiceStatusChangeCondition>()
			{
				new InvoiceStatusChangeCondition()
				{
					Status = Invoice.STATUS_PAID,
					ExceptionStatuses = new List<string>()
					{
						Invoice.EXSTATUS_FALSE
					}
				}
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
				            Status = BtcPayInvoice.STATUS_PAID,
							ExceptionStatus = Invoice.EXSTATUS_PAID_PARTIAL,
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
            
            parameters.Conditions = new List<InvoiceStatusChangeCondition>()
            {
				new InvoiceStatusChangeCondition()
				{
					Status = Invoice.STATUS_PAID,
					ExceptionStatuses = new List<string>()
					{
						Invoice.EXSTATUS_FALSE
					}
				},
				new InvoiceStatusChangeCondition()
				{
					Status = Invoice.STATUS_INVALID,
					ExceptionStatuses = new List<string>()
					{
						"paidLate"
					}
				}
            };
            
            Assert.False(await handler.IsTriggered(new InvoiceStatusChangedTrigger()
                                    {
                                        Data = new InvoiceStatusChangedTriggerData()
                                        {
                                            ExternalServiceId = "A",
                                            Invoice = new BtcPayInvoice()
                                            {
                                                Status = BtcPayInvoice.STATUS_INVALID,
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
                            Status = BtcPayInvoice.STATUS_PAID,
							ExceptionStatus = Invoice.EXSTATUS_PAID_OVER,
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
				            ExceptionStatus = Invoice.EXSTATUS_FALSE,
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
				            Status = BtcPayInvoice.STATUS_INVALID,
				            ExceptionStatus = "paidLate",
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