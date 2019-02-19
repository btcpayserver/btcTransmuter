using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BtcTransmuter.Abstractions.ExternalServices;
using BtcTransmuter.Abstractions.Triggers;
using BtcTransmuter.Data.Entities;
using BtcTransmuter.Extension.BtcPayServer.ExternalServices.BtcPayServer;
using BtcTransmuter.Extension.BtcPayServer.Triggers.InvoiceStatusChanged;
using Microsoft.Extensions.Hosting;
using NBitcoin;

namespace BtcTransmuter.Extension.BtcPayServer.HostedServices
{
    public class BtcPayInvoiceWatcherHostedService : IHostedService
    {
        private readonly IExternalServiceManager _externalServiceManager;
        private readonly ITriggerDispatcher _triggerDispatcher;
        private ConcurrentDictionary<string, BtcPayServerService> _externalServices;

        public BtcPayInvoiceWatcherHostedService(IExternalServiceManager externalServiceManager,
            ITriggerDispatcher triggerDispatcher)
        {
            _externalServiceManager = externalServiceManager;
            _triggerDispatcher = triggerDispatcher;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            var externalServices = await _externalServiceManager.GetExternalServicesData(new ExternalServicesDataQuery()
            {
                Type = new[]
                {
                    BtcPayServerService.BtcPayServerServiceType
                }
            });
            _externalServices = new ConcurrentDictionary<string, BtcPayServerService>(
                externalServices
                    .Select(service =>
                        new KeyValuePair<string, BtcPayServerService>(service.Id, new BtcPayServerService(service))));

            _externalServiceManager.ExternalServiceDataUpdated += ExternalServiceManagerOnExternalServiceUpdated;
            _ = Loop(cancellationToken);
        }

        private async Task Loop(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var tasks = _externalServices.Select(CheckInvoiceChangeInService);
                await Task.WhenAll(tasks);
                await Task.Delay(TimeSpan.FromMinutes(1), cancellationToken);
            }
        }

        private async Task CheckInvoiceChangeInService(KeyValuePair<string, BtcPayServerService> pair)
        {
            try
            {

            var (key, service) = pair;
            if (!await service.CheckAccess())
            {
                return;
            }

            var data = service.Data;
            data.LastCheck = DateTime.Now;
            if (data.MonitoredInvoiceStatuses == null)
            {
                data.MonitoredInvoiceStatuses = new Dictionary<string, string>();
            }
            var client = service.ConstructClient();

            var invoices = await client.GetInvoicesAsync<BtcPayInvoice>(data.PairedDate);

            foreach (var invoice in invoices)
            {
                //do not trigger on first run
                if (data.LastCheck.HasValue)
                {
                    if (data.MonitoredInvoiceStatuses.ContainsKey(invoice.Id))
                    {
                        if (data.MonitoredInvoiceStatuses[invoice.Id] != invoice.Status)
                        {
                            await _triggerDispatcher.DispatchTrigger(new InvoiceStatusChangedTrigger()
                            {
                                Data = new InvoiceStatusChangedTriggerData()
                                {
                                    Invoice = invoice,
                                    ExternalServiceId = key
                                }
                            });
                        }
                    }
                    else
                    {
                        await _triggerDispatcher.DispatchTrigger(new InvoiceStatusChangedTrigger()
                        {
                            Data = new InvoiceStatusChangedTriggerData()
                            {
                                Invoice = invoice,
                                ExternalServiceId = key
                            }
                        });
                    }
                }

                data.MonitoredInvoiceStatuses.AddOrReplace(invoice.Id, invoice.Status);
            }


            service.Data = data;
            await _externalServiceManager.UpdateInternalData(key, service.Data);
            
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        private void ExternalServiceManagerOnExternalServiceUpdated(object sender, UpdatedItem<ExternalServiceData> e)
        {
            if (e.Item.Type != BtcPayServerService.BtcPayServerServiceType)
            {
                return;
            }

            switch (e.Action)
            {
                case UpdatedItem<ExternalServiceData>.UpdateAction.Added:
                    _externalServices.TryAdd(e.Item.Id, new BtcPayServerService(e.Item));
                    break;
                case UpdatedItem<ExternalServiceData>.UpdateAction.Removed:
                    _externalServices.TryRemove(e.Item.Id, out var _);
                    break;
                case UpdatedItem<ExternalServiceData>.UpdateAction.Updated:
                    _externalServices.TryUpdate(e.Item.Id, new BtcPayServerService(e.Item), null);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}