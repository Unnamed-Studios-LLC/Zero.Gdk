using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Zero.Game.Common;
using Zero.Game.Shared;

namespace Zero.Game.Server
{
    public class Node
    {
        private readonly ConcurrentDictionary<uint, World> _worlds = new();
        private readonly ConcurrentDictionary<uint, Connection> _connections = new();
        private readonly ConcurrentDictionary<string, StartConnectionRequest> _connectionKeys = new();
        private readonly CancellationTokenSource _stoppingSource = new();
        private Task _networkLoopTask;
        private uint _nextConnectionId = 1;

        private readonly INetworkListener<StartConnectionRequest> _networkListener;

        internal Node(INetworkListener<StartConnectionRequest> networkListener)
        {
            _networkListener = networkListener;
        }

        public StartConnectionResponse AddConnection(StartConnectionRequest request)
        {
            if (!IPAddress.TryParse(request.ClientIp, out var remoteAddress))
            {
                return ConnectionFailReason.InvalidClientIpAddress;
            }

            if (!_worlds.ContainsKey(request.WorldId))
            {
                return ConnectionFailReason.WorldNotFound;
            }

            var key = Random.StringAlphaNumeric(20);
            if (!_connectionKeys.TryAdd(key, request))
            {
                return ConnectionFailReason.InternalError;
            }

            _networkListener.Whitelist(remoteAddress, DateTime.UtcNow.AddMilliseconds(ServerDomain.Options.ConnectionAcceptTimeoutMs));

            ServerDomain.InternalLog(LogLevel.Information, "Opened connection from {0} at port {1}", request.ClientIp, ServerDomain.Options.Networking.Port);

            return new StartConnectionResponse(null, ServerDomain.Options.Networking.Port, key);
        }

        public async Task<StartWorldResponse> AddWorldAsync(StartWorldRequest request)
        {
            var world = new World(request.WorldId, request.Data);
            bool worldValid = false;
            try
            {
                worldValid = ServerDomain.Setup.NewWorld(world);
            }
            catch (Exception e)
            {
                // error
                Debug.LogError(e, "An error occurred during {0}", nameof(ServerDomain.Setup.NewWorld));
                return WorldFailReason.NewWorldFailed;
            }

            if (!worldValid ||
                !await world.InitAsync()
                    .ConfigureAwait(false))
            {
                // error
                return WorldFailReason.InitFailed;
            }

            world.AddToNode();

            if (!_worlds.TryAdd(request.WorldId, world))
            {
                await world.DestroyAsync()
                    .ConfigureAwait(false);
                return WorldFailReason.InternalError;
            }

            ServerDomain.InternalLog(LogLevel.Information, "Added world {0}", request.WorldId);

            return new StartWorldResponse(request.WorldId);
        }

        public void RemoveConnection(uint connectionId)
        {
            if (!_connections.TryRemove(connectionId, out var connection))
            {
                return; // already removed
            }

            ServerDomain.InternalLog(LogLevel.Information, "Removed connection from {0}", connection.RemoteIp);

            connection.Close();
            connection.RemoveFromWorld();
            _ = Task.Run(() => connection.DestroyAsync());
        }

        public async Task RemoveWorldAsync(uint worldId)
        {
            if (!_worlds.TryRemove(worldId, out var world))
            {
                return;
            }

            ServerDomain.InternalLog(LogLevel.Information, "Removed world {0}", worldId);

            world.RemoveFromNode();
            await world.DestroyAsync()
                .ConfigureAwait(false);
        }

        public void Start()
        {
            _networkListener.Start(ServerDomain.Options.Networking.Port, GetKeyData, _stoppingSource.Token);
            _networkLoopTask = ReceiveClientsLoopAsync();
        }

        public void Stop()
        {
            _stoppingSource.Cancel();
            _networkListener.Stop();
            _networkLoopTask?.GetAwaiter().GetResult();
            RemoveConnections();
        }

        public void Tick(bool isViewUpdate)
        {
            ProcessReceivedActions();
            TickWorlds(isViewUpdate);
            TickConnections(isViewUpdate);
        }

        private uint GetConnectionId()
        {
            uint id;
            do
            {
                id = _nextConnectionId++;
            }
            while (id == 0 || _connections.ContainsKey(id));
            return id;
        }

        private StartConnectionRequest GetKeyData(string key)
        {
            if (!_connectionKeys.TryRemove(key, out var data))
            {
                return null;
            }
            return data;
        }

        private void ProcessReceivedActions()
        {
            var connections = _connections.Values;
            foreach (var connection in connections)
            {
                if (!connection.ConnectionActive)
                {
                    if (connection.Closed)
                    {
                        RemoveConnection(connection.Id);
                    }
                    continue;
                }

                if (connection.World == null ||
                    connection.TargetWorldId != connection.World.Id)
                {
                    if (!_worlds.TryGetValue(connection.TargetWorldId, out var world))
                    {
                        RemoveConnection(connection.Id);
                        continue;
                    }

                    connection.World?.RemoveConnection(connection);
                    world.AddConnection(connection);
                }

                connection.ProcessReceived();
            }
        }

        private async Task ReceivedConnectionAsync(INetworkClient client, StartConnectionRequest request)
        {
            if (!_worlds.ContainsKey(request.WorldId))
            {
                client?.Close();
                return;
            }

            var id = GetConnectionId();
            var connection = new Connection(id, client, request.WorldId, request.Data);
            if (!_connections.TryAdd(id, connection))
            {
                // error
                connection.Close();
                return;
            }

            var connectionValid = false;
            try
            {
                connectionValid = ServerDomain.Setup.NewConnection(connection);
            }
            catch (Exception e)
            {
                Debug.LogError(e, "An error occurred during {0}", nameof(ServerDomain.Setup.NewConnection));
                connection.Close();
                return;
            }

            if (!connectionValid)
            {
                connection.Close();
                return;
            }

            if (!await connection.InitAsync()
                    .ConfigureAwait(false))
            {
                // error
                connection.Close();
                _connections.TryRemove(connection.Id, out _);
                return;
            }

            ServerDomain.InternalLog(LogLevel.Information, "Received connection from {0}", connection.RemoteIp);
            connection.ConnectionActive = true;
            connection.StartReceive();
        }

        private async Task ReceiveClientsLoopAsync()
        {
            await foreach (var (keyResult, client) in _networkListener.ReceiveClientAsync(_stoppingSource.Token).ConfigureAwait(false))
            {
                if (_stoppingSource.IsCancellationRequested)
                {
                    break;
                }

                if (client == null)
                {
                    continue;
                }

                // use client
                _ = ReceivedConnectionAsync(client, keyResult);
            }
        }

        private async Task RemoveConnectionAsync(Connection connection)
        {
            ServerDomain.InternalLog(LogLevel.Information, "Removed connection from {0}", connection.RemoteIp);

            connection.Close();
            connection.RemoveFromWorld();
            await connection.DestroyAsync()
                .ConfigureAwait(false);
        }

        private void RemoveConnections()
        {
            var waitEvent = new ManualResetEvent(false);
            _ = RemoveConnectionsAsync(waitEvent);
            waitEvent.WaitOne(12_000);
        }

        private async Task RemoveConnectionsAsync(ManualResetEvent @event)
        {
            var connections = _connections.Values;
            var timeout = Task.Delay(10_000);
            var tasks = connections.Select(x => RemoveConnectionAsync(x))
                .Append(timeout)
                .ToList();

            while (tasks.Count > 1)
            {
                var completedTask = await Task.WhenAny(tasks)
                    .ConfigureAwait(false);
                tasks.Remove(completedTask);

                if (completedTask == timeout)
                {
                    break;
                }
            }

            @event.Set();
        }

        private void TickConnections(bool isViewUpdate)
        {
            var connections = _connections.Values;
            foreach (var connection in connections)
            {
                if (!connection.ConnectionActive)
                {
                    if (connection.Closed)
                    {
                        RemoveConnection(connection.Id);
                    }
                    continue;
                }

                if (connection.World == null)
                {
                    continue;
                }

                connection.UpdateView(isViewUpdate);
                connection.UpdateTransfer();
                connection.Send();
            }
        }

        private void TickWorlds(bool isViewUpdate)
        {
            var worlds = _worlds.Values;
            foreach (var world in worlds)
            {
                world.UpdateAll();
                if (isViewUpdate)
                {
                    world.ViewUpdateAll();
                }
            }
        }

        internal void AddMockConnection(StartConnectionRequest request)
        {
            _ = Task.Run(() => ReceivedConnectionAsync(null, request));
        }

        internal bool TryGetWorld(uint worldId, out World world)
        {
            return _worlds.TryGetValue(worldId, out world);
        }
    }
}
