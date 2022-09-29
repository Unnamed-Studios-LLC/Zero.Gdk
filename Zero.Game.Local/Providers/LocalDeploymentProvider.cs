using System.Collections.Concurrent;
using System.Threading.Tasks;
using Zero.Game.Server;

namespace Zero.Game.Local.Providers
{
    public class LocalDeploymentProvider : IDeploymentProvider
    {
        private uint _nextWorldId = 10000;
        private readonly ConcurrentDictionary<uint, uint> _usedWorldIds = new();

        public string GetHost()
        {
            return "127.0.0.1";
        }

        public Task<StartConnectionResponse> StartConnectionAsync(StartConnectionRequest request)
        {
            var response = ZeroLocal.Server.OpenConnection(request);
            if (!response.Started)
            {
                return Task.FromResult(response);
            }

            return Task.FromResult(new StartConnectionResponse(GetHost(), response.Port, response.Key));
        }

        public async Task<StartWorldResponse> StartWorldAsync(StartWorldRequest request)
        {
            if (request.WorldId == 0)
            {
                // auto generate ID
                uint id;
                do
                {
                    id = _nextWorldId++;
                }
                while (_nextWorldId < 1000 && !_usedWorldIds.TryAdd(id, id));
                request.WorldId = id;
            }
            else if (_usedWorldIds.ContainsKey(request.WorldId))
            {
                return WorldFailReason.WorldIdTaken;
            }

            var response = await ZeroLocal.Server.AddWorldAsync(request)
                .ConfigureAwait(false);
            return response;
        }

        public async Task StopWorldAsync(uint worldId)
        {
            await ZeroLocal.Server.RemoveWorldAsync(worldId)
                .ConfigureAwait(false);
            _usedWorldIds.TryRemove(worldId, out _);
        }
    }
}
