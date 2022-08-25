using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Zero.Game.Common;
using Zero.Game.Shared;

namespace Zero.Game.Client
{
    public class ZeroClientInstance
    {
        private readonly List<ClientComponent> _components = new List<ClientComponent>();
        private INetworkClient _client;
        private uint _lastReceivedBatchId;

        private List<ViewActionBatch> _receivedBatches = new List<ViewActionBatch>();
        private List<ViewActionBatch> _nextReceivedBatches = new List<ViewActionBatch>();
        private readonly object _receiveLock = new object();
        private bool _connecting = false;
        private bool _disconnected = false;
        private bool _closed = false;
        private readonly Dictionary<uint, List<IData>> _sentData = new Dictionary<uint, List<IData>>();

        public int Latency => _client?.Latency ?? -1;

        public void AddComponent(ClientComponent component)
        {
            _components.Add(component);
        }

        public void Disconnect()
        {
            _client?.Close();
            _disconnected = true;
        }

        public T GetComponent<T>() where T : ClientComponent
        {
            for (int j = 0; j < _components.Count; j++)
            {
                if (_components[j] is T tValue)
                {
                    return tValue;
                }
            }
            return default;
        }

        public void Push(uint entityId, IData data)
        {
            if (_connecting)
            {
                return;
            }

            if (!_sentData.TryGetValue(entityId, out var datas))
            {
                datas = ListCache.GetDataList();
                _sentData.Add(entityId, datas);
            }

            datas.Add(data);
        }

        public void Update(uint time)
        {
            if (_closed ||
                _connecting)
            {
                return;
            }

            for (int j = 0; j < _components.Count; j++)
            {
                _components[j].Time = time;
            }

            if (_disconnected || !_client.Connected)
            {
                _closed = true;
                Disconnect();
                for (int j = 0; j < _components.Count; j++)
                {
                    _components[j].Disconnect();
                }
                return;
            }

            for (int j = 0; j < _components.Count; j++)
            {
                _components[j].Update();
            }
            ProcessReceivedData(time);
        }

        private void Disconnect(INetworkClient client)
        {
            client.Close();
            if (_client != client)
            {
                return;
            }
            _disconnected = true;
        }

        private void FlushSentDatas(uint time, bool optional)
        {
            if (optional &&
                _sentData.Count == 0)
            {
                return;
            }

            var actions = ListCache.GetViewActionList();
            actions.AddRange(_sentData.Select(x =>
            {
                for (int j = 0; j < _components.Count; j++)
                {
                    _components[j].SentEntityData(x.Key, x.Value);
                }
                var action = ViewActionCache.GetUpdate(ObjectType.Entity, x.Key, x.Value);
                action.Aquire();
                return action;
            }));
            _sentData.Clear();

            var batch = ViewActionBatchCache.Get(_lastReceivedBatchId, time, actions);
            var buffer = PacketBufferCache.GetBuffer();
            var writer = new BitWriter(buffer.Data);
            batch.Write(writer);
            ViewActionBatchCache.Return(batch);
            buffer = writer.GetBuffer();
            _client.SendReliable(buffer);
        }

        private void ProcessActions(List<ViewAction> actions)
        {
            for (int i = 0; i < actions.Count; i++)
            {
                switch (actions[i])
                {
                    case RemoveViewAction removeAction:
                        ProcessRemoveAction(removeAction);
                        break;
                    case TransferViewAction transferAction:
                        ProcessTransferAction(transferAction);
                        break;
                    case UpdateViewAction updateAction:
                        ProcessUpdateAction(updateAction);
                        break;
                }
            }
        }

        private void ProcessBatch(ViewActionBatch batch)
        {
            ProcessActions(batch.Actions);
            _lastReceivedBatchId = batch.BatchId;
        }

        private void ProcessReceivedData(uint time)
        {
            lock (_receiveLock)
            {
                var temp = _receivedBatches;
                _receivedBatches = _nextReceivedBatches;
                _nextReceivedBatches = temp;
            }

            PreReceive();

            for (int i = 0; i < _receivedBatches.Count; i++)
            {
                var batch = _receivedBatches[i];
                ProcessBatch(batch);
            }

            PostReceive();

            // return batches
            for (int i = 0; i < _receivedBatches.Count; i++)
            {
                ViewActionBatchCache.Return(_receivedBatches[i]);
            }

            FlushSentDatas(time, _receivedBatches.Count == 0);
            _receivedBatches.Clear();
        }

        private void PostReceive()
        {
            for (int j = 0; j < _components.Count; j++)
            {
                _components[j].PostReceive();
            }
        }

        private void PreReceive()
        {
            for (int j = 0; j < _components.Count; j++)
            {
                _components[j].PreReceive();
            }
        }

        private void ProcessRemoveAction(RemoveViewAction removeAction)
        {
            for (int i = 0; i < removeAction.RemovedEntitiesCount; i++)
            {
                var id = removeAction.RemovedEntities[i];
                for (int j = 0; j < _components.Count; j++)
                {
                    _components[j].ReceivedEntityRemoved(id);
                }
            }
        }

        private void ProcessTransferAction(TransferViewAction transferAction)
        {
            for (int j = 0; j < _components.Count; j++)
            {
                _components[j].Connect();
            }
            ClientDomain.InternalLog(LogLevel.Information, "Transfer to Ip: {0}, Port: {1}", transferAction.Ip, transferAction.Port);
            _ = Task.Run(() => TransferAsync(transferAction.Ip, transferAction.Port, transferAction.Key));
        }

        private void ProcessUpdateAction(UpdateViewAction updateAction)
        {
            switch (updateAction.ObjectType)
            {
                case ObjectType.Entity:
                    for (int j = 0; j < _components.Count; j++)
                    {
                        _components[j].ReceivedEntityData(updateAction.Id, updateAction.Data);
                    }
                    break;
                case ObjectType.World:
                    for (int j = 0; j < _components.Count; j++)
                    {
                        _components[j].ReceivedWorldData(updateAction.Id, updateAction.Data);
                    }
                    break;
            }
        }

        private async Task ReceiveLoopAsync()
        {
            var client = _client;
            while (client.Connected)
            {
                var (success, received) = await client.ReceiveAsync()
                    .ConfigureAwait(false);
                if (!success)
                {
                    Disconnect(client);
                    return;
                }

                var reader = new BitReader(received.Data, 0, received.Size);
                var batch = ViewActionBatchCache.Get(reader);

                lock (_receiveLock)
                {
                    if (_client == client)
                    {
                        _nextReceivedBatches.Add(batch);
                    }
                    else
                    {
                        ViewActionBatchCache.Return(batch);
                    }
                }

                PacketBufferCache.ReturnBuffer(received);
            }
        }

        private async Task TransferAsync(string ip, int port, string key)
        {
            _connecting = true;
            if (!await ConnectAsync(ip, port, key)
                .ConfigureAwait(false))
            {
                _disconnected = true;
            }

            foreach (var batch in _nextReceivedBatches)
            {
                ViewActionBatchCache.Return(batch);
            }
            _nextReceivedBatches.Clear();
            _receivedBatches.Clear();
            _lastReceivedBatchId = 0;
            foreach (var data in _sentData.Values)
            {
                ListCache.ReturnDataList(data);
            }
            _sentData.Clear();
            Start();
            _connecting = false;
        }

        internal async Task<bool> ConnectAsync(string ip, int port, string key)
        {
            if (_client != null)
            {
                var client = _client;
                _client = null;

                client.Close();
                await Task.Delay(500)
                    .ConfigureAwait(false);
            }

            var ipAddress = IPAddress.Parse(ip);
            _client = ClientDomain.CreateNetworkClient(ipAddress.AddressFamily == AddressFamily.InterNetworkV6);

            var connectResult = await _client.ConnectAsync(ip, port, key)
                .ConfigureAwait(false);

            return connectResult;
        }

        internal void Start()
        {
            _ = Task.Run(ReceiveLoopAsync);
        }

        internal void Stop()
        {
            Disconnect();

            if (_closed)
            {
                return;
            }

            _closed = true;
            for (int j = 0; j < _components.Count; j++)
            {
                _components[j].Disconnect();
            }
        }
    }
}
