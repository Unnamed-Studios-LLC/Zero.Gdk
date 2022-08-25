using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Zero.Game.Common;
using Zero.Game.Shared;

namespace Zero.Game.Server
{
    public sealed class Connection : Entity
    {
        private readonly INetworkClient _client;
        private List<ViewActionBatch> _receivedBatches = new();
        private List<ViewActionBatch> _nextReceivedBatches = new();
        private readonly object _receiveLock = new();
        private uint _lastReceivedBatchId = uint.MaxValue;
        private uint _lastSentBatchId = uint.MaxValue;
        private readonly object _sentLock = new();
        private readonly Dictionary<uint, ViewActionBatch> _sentBatches = new();
        private readonly List<IConnectionComponent> _connectionComponents = new();
        private readonly List<IQueryComponent> _queryComponents = new();
        private int _disposed = 0;
        private bool _transferring = false;
        private StartConnectionResponse _transferResponse;

        public IReadOnlyDictionary<string, string> Data { get; private set; }
        public bool Disconnected { get; private set; } = false;
        public IPAddress RemoteIp => _client?.RemoteIp;

        internal bool ConnectionActive { get; set; } = true;
        internal bool Closed { get; set; }
        internal uint TargetWorldId { get; set; }
        internal bool Transferred { get; set; }
        internal View View { get; } = new View();

        internal Connection(uint id, INetworkClient client, uint worldId, Dictionary<string, string> data)
        {
            Id = id;
            _client = client;
            TargetWorldId = worldId;
            Data = data;
            ConnectionActive = false;
        }

        public void Close()
        {
            DisconnectClient();

            ConnectionActive = false;
            Closed = true;
        }

        public void DisconnectClient()
        {
            Disconnected = true;

            if (Interlocked.CompareExchange(ref _disposed, 1, 0) == 0)
            {
                _client?.Close();
                ViewActionBatch[] sentBatches = null;
                lock(_sentLock)
                {
                    sentBatches = _sentBatches.Values.ToArray();
                    _sentBatches.Clear();
                }

                foreach (var batch in sentBatches)
                {
                    ViewActionBatchCache.Return(batch);
                }
            }
        }

        public void GiveAuthority(uint entityId)
        {
            View.GiveAuthority(entityId);
        }

        public bool HasAuthority(uint entityId)
        {
            return View.HasAuthority(entityId);
        }

        public void TransferToWorld(uint worldId)
        {
            TransferToWorld(worldId, new Dictionary<string, string>(Data));
        }

        public void TransferToWorld(uint worldId, Dictionary<string, string> data)
        {
            if (_transferring ||
                Disconnected ||
                Closed)
            {
                return;
            }

            if (ZeroServer.Node.TryGetWorld(worldId, out var world))
            {
                TargetWorldId = world.Id;
                Data = data;
                return;
            }

            _transferring = true;
            var request = new StartConnectionRequest
            {
                WorldId = worldId,
                ClientIp = RemoteIp.ToString(),
                Data = data,
            };

            _ = Task.Run(() => TransferAsync(request));
        }

        private void PostReceive(uint time)
        {
            if (Disconnected)
            {
                return;
            }

            for (int i = 0; i < _connectionComponents.Count; i++)
            {
                _connectionComponents[i].PostReceive(time);
            }
        }

        private void PreReceive(uint time)
        {
            if (Disconnected)
            {
                return;
            }

            for (int i = 0; i < _connectionComponents.Count; i++)
            {
                _connectionComponents[i].PreReceive(time);
            }
        }

        private bool ProcessClientReceived(uint time, uint batchId)
        {
            if (Disconnected)
            {
                return false;
            }

            if (Ids.GetDifference(_lastSentBatchId, batchId) < 0)
            {
                // error
                DisconnectClient();
                return false;
            }

            var difference = Ids.GetDifference(batchId, _lastReceivedBatchId);
            if (difference <= 0)
            {
                return true;
            }

            for (uint i = 1; i <= difference; i++)
            {
                var id = _lastReceivedBatchId + i;
                ViewActionBatch sentBatch = null;
                lock (_sentLock)
                {
                    if (_sentBatches.TryGetValue(id, out sentBatch))
                    {
                        _sentBatches.Remove(id);
                    }
                }

                if (sentBatch != null)
                {
                    ProcessClientValidatedBatch(time, sentBatch);
                }
            }
            _lastReceivedBatchId = batchId;
            return true;
        }

        private void ProcessClientValidatedBatch(uint time, ViewActionBatch batch)
        {
            UpdateConnectionComponents();

            for (int i = 0; i < batch.Actions.Count; i++)
            {
                var action = batch.Actions[i];
                switch (action)
                {
                    case UpdateViewAction updateAction:
                        switch (updateAction.ObjectType)
                        {
                            case ObjectType.Entity:
                                for (int j = 0; j < _connectionComponents.Count; j++)
                                {
                                    _connectionComponents[j].ClientReceivedEntityData(time, updateAction.Id, updateAction.Data);
                                }
                                break;
                            case ObjectType.World:
                                for (int j = 0; j < _connectionComponents.Count; j++)
                                {
                                    _connectionComponents[j].ClientReceivedWorldData(time, updateAction.Id, updateAction.Data);
                                }
                                break;
                        }
                        break;
                    case RemoveViewAction removeAction:
                        for (int j = 0; j < _connectionComponents.Count; j++)
                        {
                            for (int k = 0; k < removeAction.RemovedEntitiesCount; k++)
                            {
                                _connectionComponents[j].ClientReceivedEntityRemoved(time, removeAction.RemovedEntities[k]);
                            }
                        }
                        break;
                }
            }
            ViewActionBatchCache.Return(batch);
        }

        private void ProcessTransferResponse(StartConnectionResponse response)
        {
            if (!response.Started)
            {
                ServerDomain.InternalLog(Shared.LogLevel.Error, "Transfer failed, reason: {0}", response.FailReason);
                return;
            }

            View.Transfer(response.WorkerIp, (ushort)response.Port, response.Key);
        }

        private void ProcessReceivedBatch(ViewActionBatch batch)
        {
            if (Disconnected)
            {
                return;
            }

            UpdateConnectionComponents();

            for (int i = 0; i < batch.Actions.Count; i++)
            {
                if (batch.Actions[i] is not UpdateViewAction updateAction ||
                    updateAction.ObjectType != ObjectType.Entity)
                {
                    continue;
                }

                for (int j = 0; j < _connectionComponents.Count; j++)
                {
                    _connectionComponents[j].ReceivedData(batch.Time, updateAction.Id, updateAction.Data);
                }
            }
        }

        private async Task TransferAsync(StartConnectionRequest request)
        {
            var response = await Deployment.StartConnectionAsync(request)
                .ConfigureAwait(false);
            _transferResponse = response;
        }

        private void UpdateConnectionComponents()
        {
            _connectionComponents.Clear();
            GetComponents(_connectionComponents);
        }

        private void UpdateQueryComponents()
        {
            _queryComponents.Clear();
            GetComponents(_queryComponents);
        }

        internal void ClearView()
        {
            View.Clear();
        }

        internal void ProcessReceived()
        {
            if (Disconnected)
            {
                return;
            }

            lock (_receiveLock)
            {
                (_receivedBatches, _nextReceivedBatches) = (_nextReceivedBatches, _receivedBatches);
                _nextReceivedBatches.Clear();
            }

            foreach (var batch in _receivedBatches)
            {
                PreReceive(batch.Time);
                ProcessClientReceived(batch.Time, batch.BatchId);
                ProcessReceivedBatch(batch);
                PostReceive(batch.Time);
            }

            foreach (var batch in _receivedBatches)
            {
                ViewActionBatchCache.Return(batch);
            }
        }

        internal async Task ReceiveLoopAsync()
        {
            while (_client?.Connected ?? false)
            {
                var (success, received) = await _client.ReceiveAsync()
                    .ConfigureAwait(false);
                if (!success)
                {
                    break;
                }

                var reader = new BitReader(received.Data, 0, received.Size);
                var batch = ViewActionBatchCache.Get(reader);

                lock (_receiveLock)
                {
                    _nextReceivedBatches.Add(batch);
                }

                PacketBufferCache.ReturnBuffer(received);
            }
            DisconnectClient();
        }

        internal void Send()
        {
            if (!View.HasBatch() ||
                Disconnected)
            {
                return;
            }

            var batch = View.GetBatch();

            UpdateConnectionComponents();
            for (int i = 0; i < batch.Actions.Count; i++)
            {
                switch (batch.Actions[i])
                {
                    case RemoveViewAction removeAction:
                        for (int j = 0; j < _connectionComponents.Count; j++)
                        {
                            for (int k = 0; k < removeAction.RemovedEntitiesCount; k++)
                            {
                                _connectionComponents[j].SentEntityRemoved(removeAction.RemovedEntities[k]);
                            }
                        }
                        break;
                    case UpdateViewAction updateAction:
                        switch (updateAction.ObjectType)
                        {
                            case ObjectType.Entity:
                                for (int j = 0; j < _connectionComponents.Count; j++)
                                {
                                    _connectionComponents[j].SentEntityData(updateAction.Id, updateAction.Data);
                                }
                                break;
                            case ObjectType.World:
                                for (int j = 0; j < _connectionComponents.Count; j++)
                                {
                                    _connectionComponents[j].SentWorldData(updateAction.Id, updateAction.Data);
                                }
                                break;
                        }
                        break;
                }
            }

            var buffer = PacketBufferCache.GetBuffer();
            var writer = new BitWriter(buffer.Data);
            batch.Write(writer);
            buffer = writer.GetBuffer();
            _lastSentBatchId = batch.BatchId;
            lock (_sentLock)
            {
                _sentBatches[batch.BatchId] = batch;
            }
            if (_client != null)
            {
                _client.SendReliable(buffer);
            }
            else
            {
                PacketBufferCache.ReturnBuffer(buffer);
            }
        }

        internal void StartReceive()
        {
            if (_client == null)
            {
                return;
            }

            _ = ReceiveLoopAsync();
        }

        internal void UpdateTransfer()
        {
            if (!_transferring ||
                _transferResponse == null)
            {
                return;
            }

            var response = _transferResponse;
            _transferResponse = null;
            _transferring = false;

            ProcessTransferResponse(response);
        }

        internal void UpdateView(bool viewUpdate)
        {
            if (viewUpdate)
            {
                UpdateQueryComponents();
                View.ViewUpdate(World, _queryComponents.SelectMany(x => x.GetEntitiesSafe()));
            }
            else
            {
                View.Update(World);
            }
        }
    }
}
