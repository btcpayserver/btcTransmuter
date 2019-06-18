using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BtcTransmuter.Abstractions.ExternalServices;
using BtcTransmuter.Abstractions.Triggers;
using BtcTransmuter.Data.Entities;
using BtcTransmuter.Extension.Email.ExternalServices.Pop3;
using BtcTransmuter.Extension.Email.Triggers.ReceivedEmail;
using Microsoft.Extensions.Hosting;
using MimeKit.Text;

namespace BtcTransmuter.Extension.Email.HostedServices
{
    public class Pop3ReceivingEmailHostedService : IHostedService
    {
        private readonly IExternalServiceManager _externalServiceManager;
        private readonly ITriggerDispatcher _triggerDispatcher;
        private ConcurrentDictionary<string, Pop3Service> _externalServices;

        public Pop3ReceivingEmailHostedService(IExternalServiceManager externalServiceManager,
            ITriggerDispatcher triggerDispatcher)
        {
            _externalServiceManager = externalServiceManager;
            _triggerDispatcher = triggerDispatcher;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            var pop3Services = await _externalServiceManager.GetExternalServicesData(new ExternalServicesDataQuery()
            {
                Type = new[]
                {
                    Pop3Service.Pop3ExternalServiceType
                }
            });
            _externalServices = new ConcurrentDictionary<string, Pop3Service>(
                pop3Services
                    .Select(service => new KeyValuePair<string, Pop3Service>(service.Id, new Pop3Service(service))));

            _externalServiceManager.ExternalServiceDataUpdated += ExternalServiceManagerOnExternalServiceUpdated;
            _ = Loop(cancellationToken);
        }

        private async Task Loop(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var tasks = _externalServices.Select(Selector);

                await Task.WhenAll(tasks);
                await Task.Delay(TimeSpan.FromMinutes(1), cancellationToken);
            }
        }

        private async Task Selector(KeyValuePair<string, Pop3Service> service)
        {
            var pop3Client = await service.Value.CreateClientAndConnect();

            if (pop3Client == null)
            {
                return;
            }

            var data = service.Value.GetData();



            var validEmails = pop3Client.Select((message, i) => (message, i)).Where(tuple =>
                !data.LastCheck.HasValue ||
                tuple.Item1.Date >= data.LastCheck.Value).Select(tuple => tuple.Item2).ToList();

            
            
            var emails = await pop3Client.GetMessagesAsync(validEmails);
            foreach (var email in emails)
            {
                var trigger = new ReceivedEmailTrigger()
                {
                    Data = new ReceivedEmailTriggerData()
                    {
                        Body = email.GetTextBody(TextFormat.Plain),
                        Subject = email.Subject,
                        FromEmail = email.From.ToString(),
                        ExternalServiceId = service.Key
                    }
                };
                _ = _triggerDispatcher.DispatchTrigger(trigger);
            }

            data.LastCheck = DateTime.Now;
            service.Value.SetData(data);
            await _externalServiceManager.UpdateInternalData(service.Key, data);
            await pop3Client.DisconnectAsync(true);
            pop3Client.Dispose();

        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        private void ExternalServiceManagerOnExternalServiceUpdated(object sender, UpdatedItem<ExternalServiceData> e)
        {
            if (e.Item.Type != Pop3Service.Pop3ExternalServiceType)
            {
                return;
            }

            switch (e.Action)
            {
                case UpdatedItem<ExternalServiceData>.UpdateAction.Added:
                    _externalServices.TryAdd(e.Item.Id, new Pop3Service(e.Item));
                    break;
                case UpdatedItem<ExternalServiceData>.UpdateAction.Removed:
                    _externalServices.TryRemove(e.Item.Id, out var _);
                    break;
                case UpdatedItem<ExternalServiceData>.UpdateAction.Updated:
                    _externalServices.TryUpdate(e.Item.Id, new Pop3Service(e.Item), null);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}