using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Zero.Game.Model;
using Zero.Game.Shared;
using Zero.Game.Shared.Messaging;

namespace Zero.Game.Server
{
    public sealed class Connection : IEntity
    {
        internal ushort BatchId;
        internal ulong LastReceivedKey;
        internal ulong LastSentKey;
        internal uint LastReceivedTime;

        private readonly ArrayCache<byte> _sendCache;

        internal Connection(ConnectionSocket socket, Dictionary<string, string> data, ArrayCache<byte> sendCache)
        {
            Socket = socket;
            Data = data;
            _sendCache = sendCache;
        }

        /// <summary>
        /// If the ClientReceivedMessageHandler is being called by client acknowledged data
        /// </summary>
        public bool ClientAcknowledging { get; internal set; } = true;

        /// <summary>
        /// Message handler that processes messages as if it was the remote client receiving them
        /// </summary>
        public IMessageHandler ClientReceivedMessageHandler
        {
            get => ClientReceivedMessageHandlerInternal.Implementation;
            set => ClientReceivedMessageHandlerInternal.SetImplementation(value);
        }

        /// <summary>
        /// If the connection is connected
        /// </summary>
        public bool Connected => Socket.Connected;

        /// <summary>
        /// Data set when the connection was created
        /// </summary>
        public Dictionary<string, string> Data { get; }

        /// <summary>
        /// Id of the entity created for this connection
        /// </summary>
        public uint EntityId { get; internal set; }

        /// <summary>
        /// The Entities of the current world
        /// </summary>
        public Entities Entities => World?.Entities;

        /// <summary>
        /// If this connection is connected to a world
        /// </summary>
        public bool InWorld => World != null;

        /// <summary>
        /// If the connection should not unload, even if the connection has disconnected
        /// </summary>
        public bool KeepAlive { get; set; } = false;

        /// <summary>
        /// Message handler for messages sent by the remote client
        /// </summary>
        public IMessageHandler MessageHandler
        {
            get => MessageHandlerInternal.Implementation;
            set => MessageHandlerInternal.SetImplementation(value);
        }

        /// <summary>
        /// The view query of this connection
        /// </summary>
        public ViewQuery Query { get; set; }

        /// <summary>
        /// The remote ip endpoint of this connection
        /// </summary>
        public IPEndPoint RemoteEndPoint => Socket.RemoteEndPoint;

        /// <summary>
        /// A user-assignable state object
        /// </summary>
        public object State { get; set; }

        /// <summary>
        /// If this connection is currently transferring
        /// </summary>
        public bool Transferring => TransferWorldId != 0;

        /// <summary>
        /// The world that this connection belongs to
        /// </summary>
        public World World { get; internal set; }

        internal bool CanTransmit => Connected || KeepAlive;
        internal MessageHandler ClientReceivedMessageHandlerInternal { get; set; } = new();
        internal bool HasClientReceivedMessageHandler => ClientReceivedMessageHandlerInternal.Implementation != null;
        internal bool Loaded { get; set; }
        internal bool LoadedSuccessfully { get; set; }
        internal MessageHandler MessageHandlerInternal { get; set; } = new();
        internal List<ByteBuffer> ReceiveList { get; } = new();
        internal ConnectionSocket Socket { get; }
        internal Queue<ByteBuffer> SentBuffers { get; } = new();
        internal HashSet<ulong> SentKeys { get; } = new();
        internal bool TransferInitiated { get; set; }
        internal uint TransferWorldId { get; set; }
        internal TaskCompletionSource<bool> TransferCompletion { get; set; }
        internal StartConnectionResponse TransferRemoteResponse { get; set; }
        internal bool TransferSent { get; set; }
        internal View View { get; } = new();

        /// <summary>
        /// Disconnects the remote client
        /// </summary>
        public void Disconnect() => Socket.Disconnect();

        public Task<bool> TransferToWorldAsync(uint worldId)
        {
            if (Transferring) throw new Exception("Connection is already in the process of transferring. Check 'Transferring' property for the current transfer");

            TransferInitiated = false;
            TransferCompletion = new TaskCompletionSource<bool>();
            TransferWorldId = worldId;
            return TransferCompletion.Task;
        }

        internal void AddToWorld(ServerPlugin plugin, World world)
        {
            World = world;
            EntityId = world.Entities.CreateEntity();
            world.AddConnection(this);
            Query?.NewWorld();

            try
            {
                plugin.AddToWorld(this);
            }
            catch (Exception e)
            {
                Debug.LogError(e, "An error occurred during: {0}", nameof(plugin.AddToWorld));
            }
        }

        internal void Clear()
        {
            while (SentBuffers.TryDequeue(out var buffer))
            {
                _sendCache.Return(buffer.Data);
            }
            SentKeys.Clear();
        }

        internal unsafe bool FlushClientReceived()
        {
            if (SentBuffers.Count == 0 || !HasClientReceivedMessageHandler) return true;
            var clientMessage = new ClientBatchMessage(LastSentKey, LastReceivedTime);
            return HandleClientReceived(&clientMessage);
        }

        internal void ForciblyRemove(string reason)
        {
            Debug.LogDebug("Connection {0} forcibly removed: {1}", RemoteEndPoint, reason);
            Disconnect();
        }

        internal unsafe bool HandleClientReceived(ClientBatchMessage* clientBatchMessage)
        {
            if (!HasClientReceivedMessageHandler)
            {
                LastReceivedKey = clientBatchMessage->BatchKey;
                return true;
            }

            if (clientBatchMessage->BatchKey == LastReceivedKey)
            {
                return true;
            }

            if (!SentKeys.Contains(clientBatchMessage->BatchKey))
            {
                ForciblyRemove("Invalid message data received");
                return false;
            }

            var handler = ClientReceivedMessageHandlerInternal;
            while (CanTransmit)
            {
                var sentBuffer = SentBuffers.Dequeue();
                MessageType* messageType = null;
                ServerBatchMessage* batchMessage = null;
                EntityMessage* entityMessage = null;
                byte* dataType = default;
                int* sizeRead = null;
                ushort* scalarCount = null;
                try
                {
                    fixed (byte* data = sentBuffer.Data)
                    {
                        var reader = new RawBlitReader(data, sentBuffer.Count);
                        if (!reader.Read(&sizeRead))
                        {
                            ForciblyRemove("A fault occurred while reading client received data: size");
                            return false;
                        }

                        if (!reader.Read(&messageType))
                        {
                            ForciblyRemove("A fault occurred while reading client received data: messageType");

                            return false;
                        }
                        if (!reader.Read(&batchMessage))
                        {
                            ForciblyRemove("A fault occurred while reading client received data: batch");
                            return false;
                        }

                        handler.PreHandle(clientBatchMessage->Time);
                        handler.HandleWorld(batchMessage->WorldId); // handle world

                        uint* removedEntityId = null;
                        for (int i = 0; i < batchMessage->RemovedCount; i++)
                        {
                            if (!reader.Read(&removedEntityId))
                            {
                                ForciblyRemove("A fault occurred while reading client received data: removed");
                                return false;
                            }
                            handler.HandleRemove(*removedEntityId);
                        }

                        while (reader.BytesRead < reader.Capacity)
                        {
                            if (!reader.Read(&entityMessage))
                            {
                                ForciblyRemove("A fault occurred while reading client received data: entity");
                                return false;
                            }

                            handler.HandleEntity(entityMessage->EntityId); // handle entity

                            for (uint d = 0; d < entityMessage->DataCount; d++)
                            {
                                if (!reader.Read(&dataType)) // handle data
                                {
                                    ForciblyRemove("A fault occurred while reading client received data: data");
                                    return false;
                                }

                                if (*dataType == byte.MaxValue)
                                {
                                    if (!reader.Read(&scalarCount) ||
                                        !reader.Read(&dataType))
                                    {
                                        ForciblyRemove("A fault occurred while reading client received data: data");
                                        return false;
                                    }

                                    for (int j = 0; j < *scalarCount; j++) // read/handle scalar datas
                                    {
                                        if (!handler.HandleRawData(*dataType, ref reader))
                                        {
                                            ForciblyRemove("A fault occurred while reading client received data: data");
                                            return false;
                                        }
                                    }
                                }
                                else if (!handler.HandleRawData(*dataType, ref reader)) // handle single data
                                {
                                    ForciblyRemove("A fault occurred while reading client received data: data");
                                    return false;
                                }
                            }
                        }
                        handler.PostHandle(); // post handle
                    }
                }
                catch (Exception)
                {
                    ForciblyRemove("A fault occurred while reading client received data: malformed data");
                    return false;
                }
                finally
                {
                    _sendCache.Return(sentBuffer.Data);
                }

                SentKeys.Remove(batchMessage->BatchKey);
                if (batchMessage->BatchKey == clientBatchMessage->BatchKey)
                {
                    break;
                }
            }

            LastReceivedKey = clientBatchMessage->BatchKey;
            return true;
        }

        internal void RemoveFromWorld(ServerPlugin plugin, bool removeFromWorldList)
        {
            try
            {
                plugin.RemoveFromWorld(this);
            }
            catch (Exception e)
            {
                Debug.LogError(e, "An error occurred during: {0}", nameof(plugin.RemoveFromWorld));
            }

            World.Entities.DestroyEntity(EntityId);
            if (removeFromWorldList) World.RemoveConnection(this);
            EntityId = 0;
            World = null;
        }

        internal unsafe void Send(bool hasBatch, ref ServerBatchMessage batchMessage, ref ByteBuffer buffer)
        {
            if (hasBatch && HasClientReceivedMessageHandler)
            {
                var copyBuffer = _sendCache.Get(buffer.Count);
                fixed (byte* src = buffer.Data)
                fixed (byte* dst = copyBuffer)
                {
                    Buffer.MemoryCopy(src, dst, buffer.Count, buffer.Count);
                }

                LastSentKey = batchMessage.BatchKey;
                SentKeys.Add(batchMessage.BatchKey);
                SentBuffers.Enqueue(new ByteBuffer(copyBuffer, buffer.Count));
            }
            Socket.Send(buffer);
        }

        internal void UpdateTimeout(TimeSpan timeout)
        {
            if (DateTime.UtcNow - Socket.LastReceiveUtc > timeout)
            {
                ForciblyRemove("Client timed out");
            }
        }

        internal void ViewQuery()
        {
            View.StageEntities();
            Query?.AddEntities(this);
            View.Populate();
        }
    }
}