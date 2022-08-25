using System.Net;
using UnnamedStudios.Common.Model;
using Zero.Game.Local.Services.Abstract;
using Zero.Game.Server;
using System.Net.Sockets;
using Zero.Game.Common;

namespace Zero.Game.Local.Services
{
    public class ConnectionService : IConnectionService
    {
        public ServiceResponse<StartConnectionResponse> Start(StartConnectionRequest request)
        {
            if (!IPAddress.TryParse(request.ClientIp, out var parsedIp))
            {
                return 400;
            }

            var data = new StartConnectionRequest
            {
                WorldId = request.WorldId,
                Data = request.Data,
                ClientIp = request.ClientIp,
            };

            var connectionResponse = ZeroServer.Node.AddConnection(data);
            if (!connectionResponse.Started)
            {
                Debug.LogError("Failed to start connection, reason {0}", connectionResponse?.FailReason);
                return connectionResponse;
            }

            var workerIp = parsedIp.AddressFamily == AddressFamily.InterNetworkV6 ? IPAddress.IPv6Loopback : IPAddress.Loopback;

            var response = new StartConnectionResponse
            {
                WorkerIp = workerIp.ToString(),
                Port = connectionResponse.Port,
                Key = connectionResponse.Key
            };

            return new ServiceResponse<StartConnectionResponse>(201, response);
        }
    }
}
