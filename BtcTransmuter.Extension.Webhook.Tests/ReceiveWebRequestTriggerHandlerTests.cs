using System;
using System.Threading.Tasks;
using BtcTransmuter.Abstractions.Triggers;
using BtcTransmuter.Data.Entities;
using BtcTransmuter.Data.Models;
using BtcTransmuter.Extension.Webhook.Triggers.ReceiveWebRequest;
using BtcTransmuter.Tests.Base;
using Microsoft.AspNetCore.Http;
using Xunit;
using Assert = Xunit.Assert;

namespace BtcTransmuter.Extension.Webhook.Tests
{
    public class ReceiveWebRequestTriggerHandlerTests:BaseTriggerTest<ReceiveWebRequestTriggerHandler,ReceiveWebRequestTriggerData,
        ReceiveWebRequestTriggerParameters>
    {
        [Fact]
        public async Task ReceiveWebRequestTriggerHandler_IsTriggered_TriggersCorrectly()
        {
            var triggerHandler = GetTriggerHandlerInstance();


            Assert.False(await triggerHandler.IsTriggered(new TestTrigger(), new RecipeTrigger()
            {
                TriggerId = triggerHandler.TriggerId
            }));

            Assert.False(await triggerHandler.IsTriggered(new TestTrigger(), new RecipeTrigger()
            {
                TriggerId = Guid.NewGuid().ToString()
            }));

            Assert.False(await triggerHandler.IsTriggered(new ReceiveWebRequestTrigger()
            {
                Data = new ReceiveWebRequestTriggerData() { }
            }, new RecipeTrigger()
            {
                TriggerId = triggerHandler.TriggerId
            }));

            var recipeTrigger = new RecipeTrigger()
            {
                TriggerId = triggerHandler.TriggerId,
            };
            recipeTrigger.Set(new ReceiveWebRequestTriggerParameters()
            {
                RelativeUrl = "x",
                Method = ""
            });
            Assert.False(await triggerHandler.IsTriggered(new ReceiveWebRequestTrigger()
            {
                Data = new ReceiveWebRequestTriggerData()
                {
                    RelativeUrl = "test"
                }
            }, recipeTrigger));

            Assert.True(await triggerHandler.IsTriggered(new ReceiveWebRequestTrigger()
            {
                Data = new ReceiveWebRequestTriggerData()
                {
                    RelativeUrl = "x"
                }
            }, recipeTrigger));

            recipeTrigger.Set(new ReceiveWebRequestTriggerParameters()
            {
                RelativeUrl = "x",
                Body = "hello",
                Method = "",
                BodyComparer = ReceiveWebRequestTriggerParameters.FieldComparer.Contains
            });
            Assert.False(await triggerHandler.IsTriggered(new ReceiveWebRequestTrigger()
            {
                Data = new ReceiveWebRequestTriggerData()
                {
                    RelativeUrl = "x",
                    Body = "sdas"
                }
            }, recipeTrigger));

            Assert.True(await triggerHandler.IsTriggered(new ReceiveWebRequestTrigger()
            {
                Data = new ReceiveWebRequestTriggerData()
                {
                    RelativeUrl = "x",
                    Body = "ads hello xasda"
                }
            }, recipeTrigger));


            recipeTrigger.Set(new ReceiveWebRequestTriggerParameters()
            {
                RelativeUrl = "x",
                Body = "hello",
                BodyComparer = ReceiveWebRequestTriggerParameters.FieldComparer.Equals,
                Method = ""
            });
            Assert.False(await triggerHandler.IsTriggered(new ReceiveWebRequestTrigger()
            {
                Data = new ReceiveWebRequestTriggerData()
                {
                    RelativeUrl = "x",
                    Body = "sdas"
                }
            }, recipeTrigger));

            Assert.False(await triggerHandler.IsTriggered(new ReceiveWebRequestTrigger()
            {
                Data = new ReceiveWebRequestTriggerData()
                {
                    RelativeUrl = "x",
                    Body = "ads hello xasda"
                }
            }, recipeTrigger));
            Assert.True(await triggerHandler.IsTriggered(new ReceiveWebRequestTrigger()
            {
                Data = new ReceiveWebRequestTriggerData()
                {
                    RelativeUrl = "x",
                    Body = "hello"
                }
            }, recipeTrigger));
            
            recipeTrigger.Set(new ReceiveWebRequestTriggerParameters()
            {
                RelativeUrl = "",
                Method = HttpMethods.Post
            });
            
            Assert.False(await triggerHandler.IsTriggered(new ReceiveWebRequestTrigger()
            {
                Data = new ReceiveWebRequestTriggerData()
                {
                    RelativeUrl = "x",
                    Method = HttpMethods.Post
                }
            }, recipeTrigger));
            
            Assert.True(await triggerHandler.IsTriggered(new ReceiveWebRequestTrigger()
            {
                Data = new ReceiveWebRequestTriggerData()
                {
                    RelativeUrl = "",
                    Method = HttpMethods.Post
                }
            }, recipeTrigger));
            Assert.False(await triggerHandler.IsTriggered(new ReceiveWebRequestTrigger()
            {
                Data = new ReceiveWebRequestTriggerData()
                {
                    RelativeUrl = "",
                    Method = HttpMethods.Put
                }
            }, recipeTrigger));
        }

        internal class TestTrigger : ITrigger
        {
            public string DataJson
            {
                get => "{}";
                set { }
            }

            public string Id => Guid.NewGuid().ToString();
        }

        protected override ReceiveWebRequestTriggerHandler GetTriggerHandlerInstance(params object[] setupArgs)
        {
            return new ReceiveWebRequestTriggerHandler();
        }
    }
}