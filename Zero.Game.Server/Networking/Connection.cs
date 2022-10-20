using System;
using System.Collections.Generic;
using System.Net;
using Zero.Game.Shared;

namespace Zero.Game.Server
{
    public class Connection
    {
        internal ushort BatchId;
        internal ulong LastReceivedKey;
        internal uint LastReceivedTime;

        private readonly ArrayCache<byte> _sendCache;

        internal Connection(ConnectionSocket socket, Dictionary<string, string> data, ArrayCache<byte> sendCache)
        {
            Socket = socket;
            Data = data;
            _sendCache = sendCache;
        }

        /// <summary>
        /// Message handler that processes messages as if it was the remote client receiving them
        /// </summary>
        public IMessageHandler ClientReceivedMessageHandler
        {
            get => ClientReceivedMessageHandlerInternal.Implementation;
            set => ClientReceivedMessageHandlerInternal.SetImplementation(value);
        }

        /// <summary>
        /// Data set when the connection was created
        /// </summary>
        public Dictionary<string, string> Data { get; }

        /// <summary>
        /// Id of the entity created for this connection
        /// </summary>
        public uint EntityId { get; internal set; }

        /// <summary>
        /// If this connection is connected to a world
        /// </summary>
        public bool InWorld => World != null;

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
        /// The world that this connection belongs to
        /// </summary>
        public World World { get; internal set; }

        internal MessageHandler ClientReceivedMessageHandlerInternal { get; set; } = new();
        internal bool Connected => Socket.Connected;
        internal bool HasClientReceivedMessageHandler => ClientReceivedMessageHandlerInternal.Implementation != null;
        internal bool Loaded { get; set; }
        internal MessageHandler MessageHandlerInternal { get; set; } = new();
        internal List<ByteBuffer> ReceiveList { get; } = new();
        internal ConnectionSocket Socket { get; }
        internal Queue<ByteBuffer> SentBuffers { get; } = new();
        internal HashSet<ulong> SentKeys { get; } = new();
        internal View View { get; } = new();

        /// <summary>
        /// Disconnects the remote client
        /// </summary>
        public void Disconnect()
        {
            Socket.Disconnect();
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
            while (Connected)
            {
                var sentBuffer = SentBuffers.Dequeue();
                ServerBatchMessage* batchMessage = null;
                EntityMessage* entityMessage = null;
                byte* dataType = default;
                int* sizeRead = null;
                try
                {
                    fixed (byte* data = sentBuffer.Data)
                    {
                        var reader = new RawBlitReader(data, sentBuffer.Count);
                        reader.Read(&sizeRead);
                        if (!reader.Read(&batchMessage))
                        {
                            ForciblyRemove("A fault occurred while reading client received data");
                            return false;
                        }

                        handler.PreHandle(clientBatchMessage->Time);
                        handler.HandleWorld(batchMessage->WorldId); // handle world

                        uint* removedEntityId = null;
                        for (int i = 0; i < batchMessage->RemovedCount; i++)
                        {
                            if (!reader.Read(&removedEntityId))
                            {
                                ForciblyRemove("A fault occurred while reading client received data");
                                return false;
                            }
                            handler.HandleRemove(*removedEntityId);
                        }

                        while (reader.BytesRead < reader.Capacity)
                        {
                            if (!reader.Read(&entityMessage))
                            {
                                ForciblyRemove("A fault occurred while reading client received data");
                                return false;
                            }

                            handler.HandleEntity(entityMessage->EntityId); // handle entity

                            for (uint d = 0; d < entityMessage->DataCount; d++)
                            {
                                if (!reader.Read(&dataType) ||
                                    !handler.HandleRawData(*dataType, ref reader)) // handle data
                                {
                                    ForciblyRemove("A fault occurred while reading client received data");
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

        internal unsafe void Send(ref ServerBatchMessage batchMessage, ref ByteBuffer buffer)
        {
            if (HasClientReceivedMessageHandler)
            {
                var copyBuffer = _sendCache.Get(buffer.Count);
                fixed (byte* src = buffer.Data)
                fixed (byte* dst = copyBuffer)
                {
                    Buffer.MemoryCopy(src, dst, buffer.Count, buffer.Count);
                }

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