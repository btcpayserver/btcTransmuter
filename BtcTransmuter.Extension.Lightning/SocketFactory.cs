using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using NBitcoin;
using NBitcoin.Protocol;
using NBitcoin.Protocol.Connectors;

namespace BtcTransmuter.Extension.Lightning
{
    public class SocketFactory
    {
        private readonly IConfiguration _configuration;

        private EndPoint GetSocksEndpoint()
        {
            var socksEndpointString = _configuration.GetValue<string>("socksendpoint", null);
            if (!string.IsNullOrEmpty(socksEndpointString))
            {
                if (!Utils.TryParseEndpoint(socksEndpointString, 9050, out var endpoint))
                    throw new Exception("Invalid value for socksendpoint");
                return endpoint;
            }

            return null;
        }

        public SocketFactory(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<Socket> ConnectAsync(EndPoint endPoint, CancellationToken cancellationToken)
        {
            DefaultEndpointConnector connector = new DefaultEndpointConnector();
            NodeConnectionParameters connectionParameters = new NodeConnectionParameters();
            var endpoint = GetSocksEndpoint();
            if (endpoint != null)
            {
                connectionParameters.TemplateBehaviors.Add(new NBitcoin.Protocol.Behaviors.SocksSettingsBehavior()
                {
                    SocksEndpoint = endpoint
                });
            }

            var socket = new Socket(SocketType.Stream, ProtocolType.Tcp);
            try
            {
                await connector.ConnectSocket(socket, endPoint, connectionParameters, cancellationToken);
            }
            catch
            {
                SafeCloseSocket(socket);
            }

            return socket;
        }

        internal static void SafeCloseSocket(System.Net.Sockets.Socket socket)
        {
            try
            {
                socket.Shutdown(SocketShutdown.Both);
            }
            catch
            {
            }

            try
            {
                socket.Dispose();
            }
            catch
            {
            }
        }
    }
}