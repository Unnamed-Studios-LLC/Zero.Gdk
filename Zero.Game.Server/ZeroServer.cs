using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Zero.Game.Shared;

[assembly: InternalsVisibleTo("Zero.Game.Benchmark")]
[assembly: InternalsVisibleTo("Zero.Game.Tests")]

namespace Zero.Game.Server
{
    public sealed class ZeroServer
    {
        private const string LogHeader =
@"
   \\||||||||||||||||//      //|||||||||||||\\      //|||||||||||||\\         //|||||||||||\\
    \\||||||||||||||//      //|||||||||||||||\\    //|||||||||||||||\\       //|||||||||||||\\
            //|||||//       |||||//                |||||//       \\||||     //||//       \\||\\
          //|||||//         |||||||||||||||||\\    |||||\\       //||//    |||||           |||||
        //|||||//           |||||||||||||||||//    ||||||||||||||||//      |||||           |||||
      //|||||//             |||||\\                ||||||      \\|||\\      \\||\\       //||//
     //||||||||||||||\\     \\|||||||||||||||//    ||||||       \\|||\\      \\|||||||||||||//
    //||||||||||||||||\\     \\|||||||||||||//     \\||//         \\||\\      \\|||||||||||//    v2
";

        private readonly uint _connectionStepSize;
        private readonly int _gracefulStopTimeoutMs;
        private readonly LockingList<byte[]> _writeBufferCache = new(100);
        private readonly ArrayCache<byte> _receiveCache = new(10, 100, 2);
        private readonly ArrayCache<byte> _sendCache = new(10, 100, 2);
        private readonly WorldList _worlds = new();
        private readonly List<Connection> _connections = new(100);
        private readonly ConnectionListener<StartConnectionRequest> _connectionListener;
        private readonly List<(Socket, StartConnectionRequest)> _connectionReceiveList = new();
        private readonly Ticker _ticker;
        private readonly List<WorldRequest> _worldRequests = new();
        private readonly List<Task> _loadTasks = new();
        private readonly List<Task> _unloadTasks = new();
        private readonly ManualResetEvent _waitEvent = new(false);
        private readonly ServerPlugin _plugin;
        private readonly int _sendBufferSize;
        private readonly int _receiveBufferSize;
        private readonly int _receiveMaxQueueSize;
        private uint _connectionViewOffset;
        private bool _stopped = false;
        private bool _running = false;

        internal ZeroServer(ServerPlugin plugin, ServerOptions options)
        {
            _plugin = plugin;
            _connectionStepSize = options.UpdatesPerViewUpdate;
            _gracefulStopTimeoutMs = options.GracefulStopTimeoutMs;
            _ticker = new Ticker(options.UpdateIntervalMs);
            _connectionListener = new ConnectionListener<StartConnectionRequest>(options.Port);
            _sendBufferSize = options.NetworkingOptions.ServerBufferSize;
            _receiveBufferSize = options.NetworkingOptions.ClientBufferSize;
            _receiveMaxQueueSize = options.NetworkingOptions.ServerMaxReceiveQueueSize;
        }

        public static ZeroServer Create(ILoggingProvider loggingProvider, IDeploymentProvider deploymentProvider, ServerPlugin plugin)
        {
            if (loggingProvider is null)
            {
                throw new ArgumentNullException(nameof(loggingProvider));
            }

            if (deploymentProvider is null)
            {
                throw new ArgumentNullException(nameof(deploymentProvider));
            }

            if (plugin is null)
            {
                throw new ArgumentNullException(nameof(plugin));
            }

            // set logger
            SharedDomain.SetLogger(loggingProvider);

            // get options
            var options = plugin.Options;

            // check options
            if (options == null)
            {
                options = new();
                Console.WriteLine(LogHeader);
                Debug.Log(LogLevel.Warning, "Options received from plugin are null, using default options");
            }
            else if (options.LogHeader)
            {
                Console.WriteLine(LogHeader);
            }

            // set log level
            SharedDomain.SetLogLevel(options.LogLevel);

            if (options.InternalOptions == null)
            {
                options.InternalOptions = new();
                Debug.Log(LogLevel.Warning, "Internal options from plugin are null, using default internal options");
            }

            if (options.NetworkingOptions == null)
            {
                options.NetworkingOptions = new();
                Debug.Log(LogLevel.Warning, "Networking options from plugin are null, using default networking options");
            }

            var dataBuilder = new DataBuilder();
            plugin.BuildData(dataBuilder);

            // init globals
            SharedDomain.Setup(options.InternalOptions, dataBuilder.Build());
            ServerDomain.Setup(deploymentProvider);

            var server = new ZeroServer(plugin, options);
            return server;
        }

        public async Task<StartWorldResponse> AddWorldAsync(StartWorldRequest request)
        {
            if (_stopped)
            {
                return new StartWorldResponse(WorldFailReason.InternalError);
            }

            var worldRequest = WorldRequest.CreateAdd(request.WorldId, request.Data);

            if (!_running)
            {
                await CreateWorldNowAsync(worldRequest.WorldId, worldRequest.Data, worldRequest.Completed)
                    .ConfigureAwait(false);
            }
            else
            {
                _worldRequests.Add(worldRequest);
            }

            return await worldRequest.Completed.Task
                .ConfigureAwait(false);
        }

        public StartConnectionResponse OpenConnection(StartConnectionRequest request)
        {
            if (_stopped)
            {
                return ConnectionFailReason.InternalError;
            }

            if (!IPAddress.TryParse(request.ClientIp, out var ipAddress))
            {
                return ConnectionFailReason.InvalidClientIpAddress;
            }

            return _connectionListener.OpenConnection(ipAddress, request);
        }

        public async Task RemoveWorldAsync(uint worldId)
        {
            if (_stopped)
            {
                return;
            }

            if (!_running)
            {
                await RemoveWorldNowAsync(worldId)
                    .ConfigureAwait(false);
                return;
            }

            var worldRequest = WorldRequest.CreateRemove(worldId);
            _worldRequests.Add(worldRequest);
            await worldRequest.Completed.Task
                .ConfigureAwait(false);
        }

        public void Run(CancellationToken cancellationToken)
        {
            _running = true;
            cancellationToken.Register(() => SignalStop());
            Start();

            while (!_stopped)
            {
                SetTimeVariables();
                Update();
                _ticker.WaitNext();
            }

            _running = false;
            Stop();
        }

        public void SignalStop()
        {
            _stopped = true;
            _ticker.Stop();
        }

        private void AddRemoveWorlds()
        {
            var span = CollectionsMarshal.AsSpan(_worldRequests);
            for (int i = 0; i < span.Length; i++)
            {
                ref var request = ref span[i];
                if (request.Remove)
                {
                    RemoveWorld(request.WorldId);
                }
                else
                {
                    CreateWorld(request.WorldId, request.Data, request.Completed);
                }
            }
            _worldRequests.Clear();
        }

        private void AddRemoveConnections()
        {
            RemoveConnections();
            ReceiveConnections();
        }

        private bool CreateConnection(StartConnectionRequest request, ConnectionSocket socket)
        {
            if (!_worlds.TryGet(request.WorldId, out _))
            {
                // TODO error
                return false;
            }

            var connection = new Connection(socket, request.Data ?? new Dictionary<string, string>(), _sendCache);
            _connections.Add(connection);
            _loadTasks.Add(LoadConnectionAsync(connection, request.WorldId));
            return true;
        }

        private void CreateWorld(uint worldId, Dictionary<string, string> data, TaskCompletionSource<StartWorldResponse> completion)
        {
            if (_worlds.TryGet(worldId, out _))
            {
                completion.TrySetResult(new StartWorldResponse(worldId));
                return;
            }

            var world = new World(worldId, data ?? new Dictionary<string, string>());
            _worlds.Add(world);
            _loadTasks.Add(LoadWorldAsync(world, completion));
        }

        private async Task CreateWorldNowAsync(uint worldId, Dictionary<string, string> data, TaskCompletionSource<StartWorldResponse> completion)
        {
            if (_worlds.TryGet(worldId, out _))
            {
                completion?.TrySetResult(new StartWorldResponse(worldId));
                return;
            }

            var world = new World(worldId, data ?? new Dictionary<string, string>());
            _worlds.Add(world);
            await LoadWorldAsync(world, completion);
        }

        private byte[] GetWriteBuffer()
        {
            if (_writeBufferCache.TryPop(out var buffer))
            {
                return buffer;
            }
            return new byte[_sendBufferSize];
        }

        private async Task LoadConnectionAsync(Connection connection, uint worldId)
        {
            var success = false;
            try
            {
                success = await _plugin.LoadConnectionAsync(connection);
            }
            catch (Exception e)
            {
                Debug.LogError(e, "An error occurred while loading connection: {0}", connection.RemoteEndPoint);
                success = false;
            }

            if (!success)
            {
                connection.Disconnect();
            }
            else if (!_worlds.TryGet(worldId, out var world))
            {
                _unloadTasks.Add(UnloadConnectionAsync(connection));
            }
            else
            {
                connection.AddToWorld(_plugin, world);
            }
            connection.Loaded = true;
        }

        private async Task LoadWorldAsync(World world, TaskCompletionSource<StartWorldResponse> completion)
        {
            var response = new StartWorldResponse(world.Id);
            try
            {
                if (!await _plugin.LoadWorldAsync(world))
                {
                    response.State = WorldStartState.Failed;
                    response.FailReason = WorldFailReason.LoadReturnedFalse;
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e, "An error occurred while loading world: {0}", world.Id);
                response.State = WorldStartState.Failed;
                response.FailReason = WorldFailReason.LoadThrewException;
            }
            completion?.TrySetResult(response);
        }

        private void ReceiveConnections()
        {
            _connectionListener.GetValidated(_connectionReceiveList);
            for (int i = 0; i < _connectionReceiveList.Count; i++)
            {
                var (socket, request) = _connectionReceiveList[i];
                var connectionSocket = new ConnectionSocket(socket, _receiveBufferSize, _receiveMaxQueueSize, _receiveCache, _sendCache);
                if (!CreateConnection(request, connectionSocket))
                {
                    connectionSocket.Disconnect();
                    continue;
                }

                connectionSocket.StartReceive();
            }
            _connectionReceiveList.Clear();
        }

        private void ReceiveData()
        {
            Parallel.ForEach(_connections, ReceiveData);
        }

        private unsafe void ReceiveData(Connection connection)
        {
            if (!connection.Connected)
            {
                return;
            }

            using var scope = GameSynchronizationContext.CreateScope();

            connection.Socket.Receive(connection.ReceiveList);
            var span = CollectionsMarshal.AsSpan(connection.ReceiveList);
            var handler = connection.MessageHandlerInternal;
            int i = 0;
            ClientBatchMessage* batchMessage = null;
            EntityMessage* entityMessage = null;
            byte* dataType = null;
            try
            {
                for (i = 0; i < span.Length; i++)
                {
                    ref var receiveBuffer = ref span[i];
                    fixed (byte* buffer = receiveBuffer.Data)
                    {
                        var reader = new RawBlitReader(buffer, receiveBuffer.Count);

                        if (!reader.Read(&batchMessage))
                        {
                            connection.ForciblyRemove("Received data faulted during read: batch");
                            return;
                        }

                        if (batchMessage->Time < connection.LastReceivedTime) // received time should never be less than previous times
                        {
                            connection.ForciblyRemove("Received invalid time from client");
                            return;
                        }

                        if (!connection.HandleClientReceived(batchMessage)) // handle client received
                        {
                            return;
                        }

                        handler.PreHandle(batchMessage->Time); // pre handle
                        handler.HandleWorld(batchMessage->WorldId); // handle world

                        if (!reader.Read(&entityMessage))
                        {
                            connection.ForciblyRemove("Received data faulted during read: entity");
                            return;
                        }

                        // server does not handle entity

                        for (uint d = 0; d < entityMessage->DataCount; d++)
                        {
                            if (!reader.Read(&dataType) ||
                                !handler.HandleRawData(*dataType, ref reader)) // handle data
                            {
                                connection.ForciblyRemove("Received data faulted during read: data");
                                return;
                            }
                        }

                        handler.PostHandle(); // post handle
                    }

                    _receiveCache.Return(receiveBuffer.Data);
                }
            }
            catch (Exception)
            {
                connection.ForciblyRemove("A fault occurred while reading received data: malformed data");
            }
            finally
            {
                // return other buffers
                for (int j = i; j < span.Length; j++)
                {
                    ref var otherBuffer = ref span[j];
                    _receiveCache.Return(otherBuffer.Data);
                }
                connection.ReceiveList.Clear();
            }
        }

        private void RemoveAllConnections()
        {
            foreach (var connection in _connections)
            {
                connection.Disconnect();
                connection.World.Connections.Clear();
                connection.Clear();
                _unloadTasks.Add(UnloadConnectionAsync(connection));
            }
            _connections.Clear();
        }

        private void RemoveAllWorlds()
        {
            var worlds = _worlds.GetWorldsList().ToArray();
            foreach (var world in worlds)
            {
                RemoveWorld(world.Id);
            }
        }

        private void RemoveConnections()
        {
            for (int i = 0; i < _connections.Count; i++)
            {
                var connection = _connections[i];
                if (!connection.Loaded ||
                    connection.Connected)
                {
                    continue;
                }

                _connections.RemoveAt(i);
                i--;

                connection.Clear();
                if (connection.World != null)
                {
                    connection.World.Connections.Remove(connection);
                    connection.RemoveFromWorld(_plugin);
                }
                _unloadTasks.Add(UnloadConnectionAsync(connection));
            }
        }

        private void RemoveWorld(uint worldId)
        {
            if (!_worlds.TryRemove(worldId, out var world))
            {
                return;
            }

            // remove world connections
            var connectionSpan = CollectionsMarshal.AsSpan(world.Connections);
            for (int i = 0; i < connectionSpan.Length; i++)
            {
                ref var connection = ref connectionSpan[i];
                connection.Disconnect();
                connection.Clear();
                _unloadTasks.Add(UnloadConnectionAsync(connection));
            }
            world.Connections.Clear();
            _unloadTasks.Add(UnloadWorldAsync(world));

            return;
        }

        private async Task RemoveWorldNowAsync(uint worldId)
        {
            if (!_worlds.TryRemove(worldId, out var world))
            {
                return;
            }

            // remove world connections
            for (int i = 0; i < world.Connections.Count; i++)
            {
                var connection = world.Connections[i];
                connection.Disconnect();
                connection.RemoveFromWorld(_plugin);
                connection.Clear();
                _unloadTasks.Add(UnloadConnectionAsync(connection));
            }
            world.Connections.Clear();
            await UnloadWorldAsync(world);

            return;
        }

        private void SendData()
        {
            Parallel.ForEach(_connections, SendData);
        }

        private unsafe void SendData(Connection connection)
        {
            if (!connection.Connected)
            {
                return;
            }

            using var scope = GameSynchronizationContext.CreateScope();

            var time = Time.Total;
            var world = connection.World;
            var view = connection.View;
            var writeBuffer = GetWriteBuffer();
            ServerBatchMessage batchMessage;
            EntityMessage entityMessage;
            try
            {
                fixed (byte* buffer = &writeBuffer[0])
                {
                    var writer = new BlitWriter(buffer, _sendBufferSize);
                    if (!writer.Write(0u))
                    {
                        connection.ForciblyRemove("Sent data exceeded the max send buffer");
                        return;
                    }

                    var removedCount = (ushort)view.RemovedEntities.Count;
                    batchMessage = new ServerBatchMessage(connection.BatchId, world.Id, removedCount);
                    if (!writer.Write(&batchMessage))
                    {
                        connection.ForciblyRemove("Sent data exceeded the max send buffer");
                        return;
                    }

                    if (removedCount != 0)
                    {
                        var span = CollectionsMarshal.AsSpan(view.RemovedEntities);
                        fixed (uint* removed = span)
                        {
                            if (!writer.Write(removed, removedCount))
                            {
                                connection.ForciblyRemove("Sent data exceeded the max send buffer");
                                return;
                            }
                        }
                    }

                    view.ProcessedEntities.Clear();
                    var entityCount = 0;
                    foreach (var entityId in view.QueryEntities)
                    {
                        if (!view.ProcessedEntities.Add(entityId))
                        {
                            continue; // entity already processed
                        }

                        var isNew = view.NewEntities.Contains(entityId);
                        if (!world.Entities.TryGetEntityData(entityId, out var data))
                        {
                            continue;
                        }

                        if (!isNew && !data.HasEventData(time))
                        {
                            continue;
                        }

                        entityCount++;

                        byte[] dataBuffer;
                        int dataByteLength, dataCount;
                        if (isNew)
                        {
                            dataBuffer = data.PersistentData;
                            dataByteLength = data.PersistentDataByteLength;
                            dataCount = data.PersistentDataCount;
                        }
                        else
                        {
                            dataBuffer = data.EventData;
                            dataByteLength = data.EventDataByteLength;
                            dataCount = data.EventDataCount;
                        }

                        if (dataCount > ushort.MaxValue)
                        {
                            connection.ForciblyRemove("Entity encountered with more than 65535 data values");
                            return;
                        }

                        entityMessage = new EntityMessage(entityId, (ushort)dataCount);
                        if (!writer.Write(&entityMessage))
                        {
                            connection.ForciblyRemove("Sent data exceeded the max send buffer");
                            return;
                        }

                        fixed (byte* dataBufferPtr = dataBuffer)
                        {
                            if (!writer.Write(dataBufferPtr, dataByteLength))
                            {
                                connection.ForciblyRemove("Sent data exceeded the max send buffer");
                                return;
                            }
                        }
                    }

                    connection.View.PostSend();

                    if (entityCount == 0 &&
                        removedCount == 0)
                    {
                        return; // no data to send
                    }
                    connection.BatchId++; // increment batch

                    // send data
                    var dataLength = writer.BytesWritten;
                    writer.Seek(0);
                    writer.Write(dataLength - 4);

                    // copy data to a smaller buffer
                    var sendBuffer = _sendCache.Get(dataLength);
                    fixed (byte* src = writeBuffer)
                    fixed (byte* dst = sendBuffer)
                    {
                        Buffer.MemoryCopy(src, dst, dataLength, dataLength);
                    }

                    var byteBuffer = new ByteBuffer(sendBuffer, dataLength);
                    connection.Send(ref batchMessage, ref byteBuffer);
                }
            }
            finally
            {
                if (writeBuffer != null)
                {
                    _writeBufferCache.Add(writeBuffer);
                }
            }
        }

        private void SetTimeVariables()
        {
            Time.Delta = _ticker.Delta;
            Time.LastUpdateDuration = _ticker.LastUpdateDuration;
            Time.Total = _ticker.Total;
        }

        private void Start()
        {
            Thread.CurrentThread.Priority = ThreadPriority.Highest;
            GameSynchronizationContext.InitializeOnCurrentThread();
            _connectionListener.Start();
            _ticker.Start();
        }

        private void Stop()
        {
            _connectionListener.Stop();
            WaitForTaskList(_loadTasks, 4000, 100);
            RemoveAllConnections();
            RemoveAllWorlds();
            WaitForTaskList(_unloadTasks, 4000, 100);
            GameSynchronizationContext.Close(_gracefulStopTimeoutMs);
        }

        private async Task UnloadConnectionAsync(Connection connection)
        {
            try
            {
                await _plugin.UnloadConnectionAsync(connection);
            }
            catch (Exception e)
            {
                Debug.LogError(e, "An error occurred while unloading connection: {0}", connection.RemoteEndPoint);
            }

            connection.State = null;
        }

        private async Task UnloadWorldAsync(World world)
        {
            try
            {
                await _plugin.UnloadWorldAsync(world);
            }
            catch (Exception e)
            {
                Debug.LogError(e, "An error occurred while unloading world: {0}", world.Id);
            }
            world.State = null;
            world.Dispose();
        }

        private void Update()
        {
            GameSynchronizationContext.Run();
            AddRemoveWorlds();
            AddRemoveConnections();
            UpdateTasks();
            ReceiveData();
            UpdateWorlds();
            UpdateViews();
            SendData();
        }

        private void UpdateTasks()
        {
            for (int i = 0; i < _loadTasks.Count; i++)
            {
                var task = _loadTasks[i];
                if (task.IsCompleted)
                {
                    _loadTasks.RemoveAt(i);
                    i--;
                }
            }

            for (int i = 0; i < _unloadTasks.Count; i++)
            {
                var task = _unloadTasks[i];
                if (task.IsCompleted)
                {
                    _unloadTasks.RemoveAt(i);
                    i--;
                }
            }
        }

        private void UpdateViews()
        {
            var count = _connections.Count;
            var remainder = count % _connectionStepSize;
            var connectionsToQuery = count / _connectionStepSize;
            if (_connectionViewOffset < remainder)
            {
                connectionsToQuery++;
            }

            Parallel.For(0, (int)connectionsToQuery, UpdateView);
            _connectionViewOffset = (_connectionViewOffset + 1) % _connectionStepSize;
        }

        private void UpdateView(int parallelIndex)
        {
            using var scope = GameSynchronizationContext.CreateScope();
            var connection = _connections[(int)(parallelIndex * _connectionStepSize + _connectionViewOffset)];
            connection.ViewQuery();
        }

        private void UpdateWorlds()
        {
            var worlds = _worlds.GetWorldsList();
            for (int i = 0; i < worlds.Count; i++)
            {
                worlds[i].Update();
            }
        }

        private void WaitForTaskList(List<Task> taskList, int maxTime, int pollTime)
        {
            int waitTime = 0;
            do
            {
                if (waitTime != 0)
                {
                    _waitEvent.WaitOne(pollTime);
                }

                GameSynchronizationContext.Run();

                for (int i = 0; i < taskList.Count; i++)
                {
                    var task = taskList[i];
                    if (task.IsCompleted)
                    {
                        taskList.RemoveAt(i);
                        i--;
                    }
                }

                if (taskList.Count == 0)
                {
                    break;
                }

                waitTime += pollTime;
            }
            while (waitTime <= maxTime);
        }
    }
}
