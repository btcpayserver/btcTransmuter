using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BtcTransmuter.Abstractions.ExternalServices;
using BtcTransmuter.Abstractions.Triggers;
using BtcTransmuter.Data.Entities;
using BtcTransmuter.Extension.Email.ExternalServices.Imap;
using BtcTransmuter.Extension.Email.ExternalServices.Pop3;
using BtcTransmuter.Extension.Email.Triggers.ReceivedEmail;
using MailKit;
using MailKit.Search;
using Microsoft.Extensions.Hosting;
using MimeKit.Text;

namespace BtcTransmuter.Extension.Email.HostedServices
{
    public class ImapReceivingEmailHostedService : IHostedService
    {
        private readonly IExternalServiceManager _externalServiceManager;
        private readonly ITriggerDispatcher _triggerDispatcher;
        private ConcurrentDictionary<string, ImapService> _externalServices;

        public ImapReceivingEmailHostedService(IExternalServiceManager externalServiceManager,
            ITriggerDispatcher triggerDispatcher)
        {
            _externalServiceManager = externalServiceManager;
            _triggerDispatcher = triggerDispatcher;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            var services = await _externalServiceManager.GetExternalServicesData(new ExternalServicesDataQuery()
            {
                Type = new[]
                {
                    ImapService.ImapExternalServiceType
                }
            });
            _externalServices = new ConcurrentDictionary<string, ImapService>(
                services
                    .Select(service => new KeyValuePair<string, ImapService>(service.Id, new ImapService(service))));

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

        private async Task Selector(KeyValuePair<string, ImapService> service)
        {
            var client = await service.Value.CreateClientAndConnect();

            if (client == null)
            {
                return;
            }

            var data = service.Value.GetData();

            var inbox = client.Inbox;
            await inbox.OpenAsync(FolderAccess.ReadOnly);

            var emailIds =
                await inbox.SearchAsync(new DateSearchQuery(SearchTerm.DeliveredAfter,
                    data.LastCheck.GetValueOrDefault(data.PairedDate)));


            foreach (var emailId in emailIds)
            {
                var email = await inbox.GetMessageAsync(emailId);
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
                _ =  _triggerDispatcher.DispatchTrigger(trigger);
            }

            data.LastCheck = DateTime.Now;
            service.Value.SetData(data);
            await _externalServiceManager.UpdateInternalData(service.Key, data);
            await client.DisconnectAsync(true);
            client.Dispose();
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
                    _externalServices.TryAdd(e.Item.Id, new ImapService(e.Item));
                    break;
                case UpdatedItem<ExternalServiceData>.UpdateAction.Removed:
                    _externalServices.TryRemove(e.Item.Id, out var _);
                    break;
                case UpdatedItem<ExternalServiceData>.UpdateAction.Updated:
                    _externalServices.TryUpdate(e.Item.Id, new ImapService(e.Item), null);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}