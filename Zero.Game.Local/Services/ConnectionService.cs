using System.Net;
using UnnamedStudios.Common.Model;
using System.Net.Sockets;
using Zero.Game.Shared;
using Zero.Game.Model;
using Zero.Game.Server;
using System;
using System.Threading.Tasks;

namespace Zero.Game.Local.Services
{
    public class ConnectionService
    {
        private static readonly string s_ipv6Loopback = IPAddress.IPv6Loopback.ToString();
        private static readonly string s_ipv4Loopback = IPAddress.Loopback.ToString();

        private readonly ServerPlugin _plugin;

        public ConnectionService(ServerPlugin plugin)
        {
            _plugin = plugin;
        }

        public async Task<ServiceResponse<StartConnectionResponse>> StartAsync(StartConnectionRequest request)
        {
            if (!IPAddress.TryParse(request.ClientIp, out var parsedIp))
            {
                return 400;
            }

            try
            {
                await _plugin.OnStartConnectionAsync(request);
            }
            catch (Exception e)
            {
                Debug.LogError(e, "An error occurred during {0}", nameof(_plugin.OnStartConnectionAsync));
                return new StartConnectionResponse(ConnectionFailReason.OnStartConnectionException);
            }

            if (request.WorldId == 0)
            {
                return new StartConnectionResponse(ConnectionFailReason.WorldNotFound);
            }

            var connectionResponse = ZeroLocal.Server.OpenConnection(request);
            if (!connectionResponse.Started)
            {
                Debug.LogError("Failed to start connection, reason {0}", connectionResponse?.FailReason);
                return connectionResponse;
            }

            connectionResponse.WorkerIp = parsedIp.AddressFamily == AddressFamily.InterNetworkV6 ? s_ipv6Loopback : s_ipv4Loopback;
            return new ServiceResponse<StartConnectionResponse>(201, connectionResponse);
        }
    }
}
