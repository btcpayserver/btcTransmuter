using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using BtcTransmuter.Abstractions.ExternalServices;
using BtcTransmuter.Data.Entities;
using BtcTransmuter.Extension.NBXplorer.Models;
using BtcTransmuter.Extension.NBXplorer.Services;
using BTCPayServer.Lightning;
using Microsoft.AspNetCore.Http;

namespace BtcTransmuter.Extension.Lightning.ExternalServices.LightningNode
{
    public class LightningNodeService : BaseExternalService<LightningNodeExternalServiceData>
    {
        private readonly NBXplorerClientProvider _nbXplorerClientProvider;
        private readonly NBXplorerSummaryProvider _nbXplorerSummaryProvider;
        private readonly SocketFactory _socketFactory;
        public const string LightningNodeServiceType = "LightningNodeExternalService";
        public override string ExternalServiceType => LightningNodeServiceType;

        public override string Name => "LightningNode External Service";
        public override string Description => "Integrate lightning nodes";
        public override string ViewPartial => "ViewLightningNodeExternalService";
        public override string ControllerName => "LightningNode";
        public static int LIGHTNING_TIMEOUT = 5000;

        public LightningNodeService() : base()
        {
        }

        public LightningNodeService(ExternalServiceData data, NBXplorerClientProvider nbXplorerClientProvider,
            NBXplorerSummaryProvider nbXplorerSummaryProvider, SocketFactory socketFactory) : base(data)
        {
            _nbXplorerClientProvider = nbXplorerClientProvider;
            _nbXplorerSummaryProvider = nbXplorerSummaryProvider;
            _socketFactory = socketFactory;
        }


        public ILightningClient ConstructClient(
            LightningNodeExternalServiceData lightningNodeExternalServiceData = null)
        {
            var data = lightningNodeExternalServiceData ?? GetData();

            var client = _nbXplorerClientProvider.GetClient(data.CryptoCode);
            return client == null ? null : LightningClientFactory.CreateClient(data.ConnectionString, client.Network.NBitcoinNetwork);
        }

        public async Task<bool> TestAccess(bool isOnion)
        {
            var data = GetData();
            var summary = _nbXplorerSummaryProvider.GetSummary(data.CryptoCode);
            if (summary.State != NBXplorerState.Ready)
            {
                return false;
            }

            try
            {
                using (var cts = new CancellationTokenSource(LIGHTNING_TIMEOUT))
                {
                    var client = ConstructClient(data);
                    LightningNodeInformation info = null;
                    try
                    {
                        info = await client.GetInfo(cts.Token);
                    }
                    catch (OperationCanceledException) when (cts.IsCancellationRequested)
                    {
                        throw new Exception($"The lightning node did not reply in a timely manner");
                    }
                    catch (Exception ex)
                    {
                        throw new Exception($"Error while connecting to the API ({ex.Message})");
                    }

                    var nodeInfo = info.NodeInfoList.FirstOrDefault(i => i.IsTor == isOnion) ??
                                   info.NodeInfoList.FirstOrDefault();
                    if (nodeInfo == null)
                    {
                        throw new Exception($"No lightning node public address has been configured");
                    }

                    var blocksGap = summary.Status.ChainHeight - info.BlockHeight;
                    if (blocksGap > 10)
                    {
                        throw new Exception($"The lightning node is not synched ({blocksGap} blocks left)");
                    }

                    if (!EndPointParser.TryParse(nodeInfo.Host, nodeInfo.Port, out var endpoint))
                        throw new Exception($"Could not parse the endpoint {nodeInfo.Host}");

                    using (await _socketFactory.ConnectAsync(endpoint,  cts.Token))
                    {
                    }
                }

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }


        public Task TestConnection(NodeInfo nodeInfo, CancellationToken cancellation)
        {
            try
            {
                if (!EndPointParser.TryParse(nodeInfo.Host, nodeInfo.Port, out var endpoint))
                    throw new Exception($"Could not parse the endpoint {nodeInfo.Host}");

               
            }
            catch (Exception ex)
            {
                throw new Exception(
                    $"Error while connecting to the lightning node via {nodeInfo.Host}:{nodeInfo.Port} ({ex.Message})");
            }
            
            return Task.CompletedTask;
        }
    }

    public static class EndPointParser
    {
        public static bool TryParse(string hostPort, out EndPoint endpoint)
        {
            if (hostPort == null)
                throw new ArgumentNullException(nameof(hostPort));
            endpoint = null;
            var index = hostPort.LastIndexOf(':');
            if (index == -1)
                return false;
            var portStr = hostPort.Substring(index + 1);
            if (!ushort.TryParse(portStr, out var port))
                return false;
            return TryParse(hostPort.Substring(0, index), port, out endpoint);
        }

        public static bool TryParse(string host, int port, out EndPoint endpoint)
        {
            if (host == null)
                throw new ArgumentNullException(nameof(host));
            endpoint = null;
            if (IPAddress.TryParse(host, out var address))
                endpoint = new IPEndPoint(address, port);
            else if (host.EndsWith(".onion", StringComparison.OrdinalIgnoreCase))
                endpoint = new OnionEndpoint(host, port);
            else
            {
                if (Uri.CheckHostName(host) != UriHostNameType.Dns)
                    return false;
                endpoint = new DnsEndPoint(host, port);
            }

            return true;
        }
    }

    public class OnionEndpoint : DnsEndPoint
    {
        public OnionEndpoint(string host, int port) : base(host, port)
        {
        }
    }

    public static class RequestExtensions
    {
        public static bool IsOnion(this HttpRequest request)
        {
            if (request?.Host.Host == null)
                return false;
            return request.Host.Host.EndsWith(".onion", StringComparison.OrdinalIgnoreCase);
        }
    }
}