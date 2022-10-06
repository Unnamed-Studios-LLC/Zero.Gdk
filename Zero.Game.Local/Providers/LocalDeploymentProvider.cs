using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Zero.Game.Model;
using Zero.Game.Server;
using Zero.Game.Shared;

namespace Zero.Game.Local.Providers
{
    public class LocalDeploymentProvider : IDeploymentProvider
    {
        private uint _nextWorldId = 10000;
        private readonly ConcurrentDictionary<uint, WorldInfo> _worldInfos = new();
        private readonly ServerPlugin _plugin;

        public LocalDeploymentProvider(ServerPlugin plugin)
        {
            _plugin = plugin;
        }

        public WorldInfo[] GetAllWorldInfos()
        {
            return _worldInfos.Values.ToArray();
        }

        public void GetAllWorldInfos(List<WorldInfo> outputList)
        {
            outputList.AddRange(_worldInfos.Select(x => x.Value));
        }

        public string GetHost()
        {
            return "127.0.0.1";
        }

        public void ReportConnectionCount(uint worldId, int count)
        {
            if (!_worldInfos.ContainsKey(worldId))
            {
                return;
            }
            _worldInfos[worldId] = new WorldInfo(worldId, count);
        }

        public async Task<StartConnectionResponse> StartConnectionAsync(StartConnectionRequest request)
        {
            try
            {
                await _plugin.OnStartConnectionAsync(request);
            }
            catch (Exception e)
            {
                Debug.LogError(e, "An error occurred during {0}", nameof(_plugin.OnStartConnectionAsync));
                return ConnectionFailReason.OnStartConnectionException;
            }

            var response = ZeroLocal.Server.OpenConnection(request);
            if (!response.Started)
            {
                return response;
            }

            return new StartConnectionResponse(GetHost(), response.Port, response.Key);
        }

        public async Task<StartWorldResponse> StartWorldAsync(StartWorldRequest request)
        {
            if (!ResolveWorldId(request))
            {
                return WorldFailReason.WorldIdTaken;
            }

            var worldId = request.WorldId;
            try
            {
                await _plugin.OnStartWorldAsync(request);
            }
            catch (Exception e)
            {
                Debug.LogError(e, "An error occurred during {0}", nameof(_plugin.OnStartWorldAsync));
                _worldInfos.TryRemove(request.WorldId, out _);
                return WorldFailReason.OnStartWorldException;
            }

            if (request.WorldId != worldId) // world id was changed
            {
                Debug.LogError("WorldId was changed during {0}", nameof(_plugin.OnStartWorldAsync));
                return WorldFailReason.OnStartWorldException;
            }

            var response = await ZeroLocal.Server.AddWorldAsync(request)
                .ConfigureAwait(false);
            if (response.State != WorldStartState.Started)
            {
                _worldInfos.TryRemove(request.WorldId, out _);
            }
            return response;
        }

        public async Task StopWorldAsync(uint worldId)
        {
            if (!_worldInfos.TryRemove(worldId, out _))
            {
                return;
            }

            try
            {
                await _plugin.OnStopWorldAsync(worldId);
            }
            catch (Exception e)
            {
                Debug.LogError(e, "An error occurred during {0}", nameof(_plugin.OnStopWorldAsync));
            }

            await ZeroLocal.Server.RemoveWorldAsync(worldId)
                .ConfigureAwait(false);
        }

        public bool TryGet(uint worldId, out WorldInfo worldInfo)
        {
            return _worldInfos.TryGetValue(worldId, out worldInfo);
        }

        private bool ResolveWorldId(StartWorldRequest request)
        {
            if (request.WorldId == 0)
            {
                // auto generate ID
                uint id;
                do
                {
                    id = _nextWorldId++;
                }
                while (id < 1000 || !_worldInfos.TryAdd(id, new WorldInfo(id, 0)));
                request.WorldId = id;
            }
            else if (!_worldInfos.TryAdd(request.WorldId, new WorldInfo(request.WorldId, 0)))
            {
                return false;
            }
            return true;
        }
    }
}
