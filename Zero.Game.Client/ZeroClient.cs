using System;
using System.Collections.Generic;
using System.Net;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Zero.Game.Shared;

[assembly: InternalsVisibleTo("Zero.Game.Tests")]

namespace Zero.Game.Client
{
    public sealed unsafe class ZeroClient
    {
        private enum ConnectionState
        {
            None,
            Connecting,
            Connected,
            Disconnected
        }

        private readonly ClientPlugin _plugin;
        private readonly MessageHandler _messageHandler;
        private readonly int _sendBufferSize;
        private readonly int _receiveBufferSize;
        private readonly int _receiveMaxQueueSize;
        private readonly List<ByteBuffer> _receiveList = new List<ByteBuffer>(100);
        private readonly byte[] _writeBuffer;
        private readonly ArrayCache<byte> _bufferCache = new ArrayCache<byte>(10, 100, 2);
        private readonly EntityData _dataToSend;
        private ConnectionSocket _socket;
        private ConnectionState _state = ConnectionState.None;
        private bool _sendRequired;
        private ulong _lastReceivedBatchKey;
        private long _eventTime;

        public ZeroClient(ClientPlugin plugin, ClientOptions options, IMessageHandler messageHandler)
        {
            _plugin = plugin;
            _messageHandler = new MessageHandler();
            _messageHandler.SetImplementation(messageHandler);
            _sendBufferSize = options.NetworkingOptions.ClientBufferSize;
            _writeBuffer = new byte[_sendBufferSize];
            _receiveBufferSize = options.NetworkingOptions.ServerBufferSize;
            _receiveMaxQueueSize = options.NetworkingOptions.ClientMaxReceiveQueueSize;
            _dataToSend = new EntityData(100, _bufferCache);
        }

        public bool Connected => _state == ConnectionState.Connected;
        public IPEndPoint RemoteEndPoint => _socket.RemoteEndPoint;

        public static ZeroClient Create(IPAddress address, int port, string key, IMessageHandler messageHandler, ILoggingProvider loggingProvider, ClientPlugin plugin)
        {
            if (address is null)
            {
                throw new ArgumentNullException(nameof(address));
            }

            if (key is null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            if (loggingProvider is null)
            {
                throw new ArgumentNullException(nameof(loggingProvider));
            }

            if (plugin is null)
            {
                throw new ArgumentNullException(nameof(plugin));
            }

            // get options
            var options = plugin.Options;

            // set logger
            SharedDomain.SetLogger(loggingProvider, options.LogLevel);

            var dataBuilder = new DataBuilder();
            plugin.BuildData(dataBuilder);

            // init globals
            SharedDomain.Setup(options.InternalOptions, dataBuilder.Build());

            var client = new ZeroClient(plugin, options, messageHandler);
            client.Start(address, port, key);
            return client;
        }

        public void Disconnect()
        {
            SetDisconnected();
        }

        public void Push<T>(T data) where T : unmanaged
        {
            ThrowHelper.ThrowIfDataNotDefined<T>();
            _dataToSend.PushEvent(_eventTime, &data);
        }

        public void Update(uint time)
        {
            UpdateState();

            if (_state == ConnectionState.Disconnected)
            {
                return;
            }

            ReceiveData(time);
            SendData(time);
        }

        private void ForciblyDisconnect(string reason)
        {
            if (_state != ConnectionState.Disconnected)
            {
                return;
            }

            Debug.LogDebug("Forcibly disconnected from server: {0}", reason);
            Disconnect();
        }

        private void ReceiveData(uint time)
        {
            _socket.Receive(_receiveList);
            _sendRequired = _receiveList.Count != 0;

            var handler = _messageHandler;
            ServerBatchMessage batchMessage = default;
            EntityMessage entityMessage = default;
            byte dataType = 0;
            int i = 0;
            try
            {
                for (i = 0; i < _receiveList.Count; i++)
                {
                    var receiveBuffer = _receiveList[i];
                    fixed (byte* buffer = receiveBuffer.Data)
                    {
                        var reader = new BlitReader(buffer, receiveBuffer.Count);

                        if (!reader.Read(&batchMessage))
                        {
                            ForciblyDisconnect("Received data faulted during read");
                            return;
                        }
                        _lastReceivedBatchKey = batchMessage.BatchKey;

                        handler.PreHandle(time); // pre handle
                        handler.HandleWorld(batchMessage.WorldId); // handle world

                        uint removedEntityId = 0;
                        for (int j = 0; j < batchMessage.RemovedCount; j++)
                        {
                            if (!reader.Read(&removedEntityId))
                            {
                                ForciblyDisconnect("A fault occurred while reading client received data");
                                return;
                            }
                            handler.RemoveEntity(removedEntityId);
                        }

                        while (reader.BytesRead < reader.Capacity)
                        {
                            if (!reader.Read(&entityMessage))
                            {
                                ForciblyDisconnect("Received data faulted during read");
                                return;
                            }

                            handler.HandleEntity(entityMessage.EntityId); // handle entity

                            for (uint d = 0; d < entityMessage.DataCount; d++)
                            {
                                if (!reader.Read(&dataType) ||
                                    !handler.HandleData(dataType, ref reader)) // handle data
                                {
                                    ForciblyDisconnect("Received data faulted during read");
                                    return;
                                }
                            }
                        }

                        handler.PostHandle(); // post handle
                    }

                    _bufferCache.Return(receiveBuffer.Data);
                }
            }
            catch (Exception)
            {
                ForciblyDisconnect("A fault occurred while reading client received data: malformed data");
            }
            finally
            {
                // return other buffers
                for (int j = i; j < _receiveList.Count; j++)
                {
                    var otherBuffer = _receiveList[j];
                    _bufferCache.Return(otherBuffer.Data);
                }
                _receiveList.Clear();
            }
        }

        private void SendData(uint time)
        {
            try
            {
                if ((!_sendRequired && !_dataToSend.HasEventData(_eventTime)) || 
                    _lastReceivedBatchKey == 0)
                {
                    return;
                }

                var batchMessage = new ClientBatchMessage(_lastReceivedBatchKey, time);
                EntityMessage entityMessage;
                fixed (byte* buffer = _writeBuffer)
                {
                    var writer = new BlitWriter(buffer, _sendBufferSize);
                    if (!writer.Write(0u) || // write size space
                        !writer.Write(&batchMessage)) // write batch
                    {
                        ForciblyDisconnect("Sent data exceeded the max send buffer");
                        return;
                    }

                    if (_dataToSend.HasEventData(_eventTime))
                    {
                        var dataBuffer = _dataToSend.EventData;
                        var dataByteLength = _dataToSend.EventDataByteLength;
                        var dataCount = _dataToSend.EventDataCount;

                        if (dataCount > ushort.MaxValue)
                        {
                            ForciblyDisconnect("Entity encountered with more than 65535 data values");
                            return;
                        }

                        entityMessage = new EntityMessage(0, (ushort)dataCount);
                        if (!writer.Write(&entityMessage)) // write entity
                        {
                            ForciblyDisconnect("Sent data exceeded the max send buffer");
                            return;
                        }

                        fixed (byte* dataBufferPtr = dataBuffer)
                        {
                            if (!writer.Write(dataBufferPtr, dataByteLength))
                            {
                                ForciblyDisconnect("Sent data exceeded the max send buffer");
                                return;
                            }
                        }
                    }
                    else
                    {
                        entityMessage = new EntityMessage(0, 0);
                        if (!writer.Write(&entityMessage)) // write entity
                        {
                            ForciblyDisconnect("Sent data exceeded the max send buffer");
                            return;
                        }
                    }

                    var dataLength = writer.BytesWritten;
                    writer.Seek(0);
                    writer.Write(dataLength - 4);

                    // copy data to a smaller buffer
                    var sendBuffer = _bufferCache.Get(dataLength);

                    fixed (byte* src = _writeBuffer)
                    fixed (byte* dst = sendBuffer)
                    {
                        Buffer.MemoryCopy(src, dst, dataLength, dataLength);
                    }

                    _socket.Send(new ByteBuffer(sendBuffer, dataLength));
                }
            }
            catch (Exception)
            {
                ForciblyDisconnect("A fault occurred while writing client send data");
            }
            finally
            {
                _dataToSend.Clear();
                _eventTime++;
            }
        }

        private void SetConnected()
        {
            if (_state == ConnectionState.Connected)
            {
                return;
            }
            _state = ConnectionState.Connected;

            try
            {
                _plugin.Connected();
            }
            catch (Exception e)
            {
                Debug.LogError(e, "An error occurred during {0}", nameof(_plugin.Connected));
            }
        }

        private void SetConnecting()
        {
            if (_state == ConnectionState.Connecting)
            {
                return;
            }
            _state = ConnectionState.Connecting;

            try
            {
                _plugin.Connecting();
            }
            catch (Exception e)
            {
                Debug.LogError(e, "An error occurred during {0}", nameof(_plugin.Connected));
            }
        }

        private void SetDisconnected()
        {
            if (_state == ConnectionState.Disconnected)
            {
                return;
            }
            _state = ConnectionState.Disconnected;

            try
            {
                _plugin.Disconnected();
            }
            catch (Exception e)
            {
                Debug.LogError(e, "An error occurred during {0}", nameof(_plugin.Disconnected));
            }

            _socket?.Disconnect();
        }

        private void Start(IPAddress address, int port, string key)
        {
            _socket = new ConnectionSocket(_receiveBufferSize, _receiveMaxQueueSize, _bufferCache, _bufferCache);
            _socket.Connect(address, port, key);
        }

        private void UpdateState()
        {
            if (!_socket.Connecting && !_socket.Connected)
            {
                SetDisconnected();
            }
            else if (_socket.Connecting)
            {
                SetConnecting();
            }
            else if (_socket.Connected)
            {
                SetConnected();
            }
        }
    }
}
