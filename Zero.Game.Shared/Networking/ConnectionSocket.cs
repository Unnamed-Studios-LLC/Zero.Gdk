using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Zero.Game.Shared
{
    internal sealed unsafe class ConnectionSocket
    {
        private const int SizeLength = 4;
        private static readonly Task<bool> s_syncCompletedFalse = Task.FromResult(false);
        private static readonly Task<bool> s_syncCompletedTrue = Task.FromResult(true);

        private readonly Socket _socket;
        private readonly object _bufferLock = new object();
        private readonly int _maxReceiveSize;
        private readonly int _maxReceiveQueueSize;
        private readonly byte[] _sizeBuffer = new byte[4];

        private int _received = 0;
        private int _receivedSize = -1;
        private readonly SocketAsyncEventArgs _receiveArgs = new SocketAsyncEventArgs();
        private readonly ArrayCache<byte> _receiveCache;
        private readonly object _receiveLock = new object();
        private readonly Queue<ByteBuffer> _receiveQueue = new Queue<ByteBuffer>();
        private int _receiveQueueSize;

        private readonly SocketAsyncEventArgs _sendArgs = new SocketAsyncEventArgs();
        private readonly ArrayCache<byte> _sendCache;
        private readonly object _sendLock = new object();
        private readonly Queue<ByteBuffer> _sendQueue = new Queue<ByteBuffer>();
        private bool _sending = false;
        private byte[] _key;
        private int _closed;

        public ConnectionSocket(int maxReceiveSize, int maxReceiveQueueSize, ArrayCache<byte> receiveCache, ArrayCache<byte> sendCache) : this(null, maxReceiveSize, maxReceiveQueueSize, receiveCache, sendCache) { }

        public ConnectionSocket(Socket socket, int maxReceiveSize, int maxReceiveQueueSize, ArrayCache<byte> receiveCache, ArrayCache<byte> sendCache)
        {
            _socket = socket ?? new Socket(AddressFamily.InterNetworkV6, SocketType.Stream, ProtocolType.Tcp)
            {
                DualMode = true
            };
            _socket.NoDelay = true;
            RemoteEndPoint = (IPEndPoint)_socket.RemoteEndPoint;

            _maxReceiveSize = maxReceiveSize;
            _maxReceiveQueueSize = maxReceiveQueueSize;

            _receiveCache = receiveCache;
            _sendCache = sendCache;

            _receiveArgs.Completed += ProcessReceived;
            _receiveArgs.SetBuffer(0, 0);

            _sendArgs.Completed += ProcessSent;
        }


        public bool Connecting { get; private set; }
        public bool Connected => _socket.Connected;
        public IPEndPoint RemoteEndPoint { get; private set; }

        private bool SizeReceived => _receivedSize >= 0;
        private int SizeToReceive => SizeReceived ? _receivedSize : SizeLength;

        public void Connect(IPAddress address, int port, string key)
        {
            if (Connecting)
            {
                return;
            }

            Connecting = true;
            _key = Encoding.ASCII.GetBytes(key);
            RemoteEndPoint = new IPEndPoint(address, port);
            var connectArgs = new SocketAsyncEventArgs
            {
                RemoteEndPoint = RemoteEndPoint
            };
            connectArgs.SetBuffer(0, 0);
            connectArgs.Completed += ProcessConnect;

            if (!_socket.ConnectAsync(connectArgs))
            {
                ProcessConnect(_socket, connectArgs);
            }
        }

        public void Disconnect()
        {
            if (Interlocked.CompareExchange(ref _closed, 1, 0) == 1)
            {
                return;
            }

            try
            {
                _socket.Shutdown(SocketShutdown.Both);
                _socket.Close();
            }
            catch (ObjectDisposedException) { }
            catch (Exception e)
            {
                Debug.LogError(e, "An error occurred during {0}", nameof(Disconnect));
            }
        }

        public void StartReceive()
        {
            if (_closed != 0)
            {
                return;
            }

            try
            {
                _receiveArgs.SetBuffer(_sizeBuffer, 0, 4);
                if (!_socket.ReceiveAsync(_receiveArgs))
                {
                    ThreadPool.QueueUserWorkItem(x => ProcessReceived(null, (SocketAsyncEventArgs)x), _receiveArgs);
                }
            }
            catch (ObjectDisposedException) { }
            catch (Exception e)
            {
                Debug.LogError(e, $"An error occurred during {nameof(StartReceive)}");
                Disconnect();
            }
        }

        public void Receive(List<ByteBuffer> bufferList)
        {
            lock (_receiveLock)
            {
                while (_receiveQueue.Count > 0)
                {
                    var buffer = _receiveQueue.Dequeue();
                    _receiveQueueSize -= buffer.Count;
                    bufferList.Add(buffer);
                }
            }
        }

        public void Send(ByteBuffer buffer)
        {
            lock (_sendLock)
            {
                _sendQueue.Enqueue(buffer);
                if (_sending)
                {
                    return;
                }
                _sending = true;
            }

            ThreadPool.QueueUserWorkItem(x => SendNext(), null);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int ReadInt32LittleEndian(byte* data)
        {
            if (BitConverter.IsLittleEndian)
            {
                return *(int*)data;
            }

            var value = *(uint*)data;
            return (int)(RotateRight(value & 0x00FF00FFu, 8) // xx zz
                + RotateLeft(value & 0xFF00FF00u, 8)); // ww yy
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint RotateLeft(uint value, int offset) => (value << offset) | (value >> (32 - offset));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static uint RotateRight(uint value, int offset) => (value >> offset) | (value << (32 - offset));

        private void ProcessConnect(object sender, SocketAsyncEventArgs connectArgs)
        {
            if (connectArgs.SocketError == SocketError.Success)
            {
                Send(new ByteBuffer(_key, _key.Length));
                StartReceive();
            }
            else
            {
                Debug.LogError("Failed to connect to remote host: {0}", connectArgs.SocketError);
            }
            Connecting = false;
        }

        private void ProcessReceived(object sender, SocketAsyncEventArgs args)
        {
            try
            {
                while (Connected)
                {
                    if (args.BytesTransferred == 0)
                    {
                        Disconnect();
                        return;
                    }

                    _received += args.BytesTransferred;

                    if (_received == SizeToReceive)
                    {
                        if (!SizeReceived)
                        {
                            // size info received
                            fixed (byte* data = _sizeBuffer)
                            {
                                _receivedSize = ReadInt32LittleEndian(data);
                            }

                            if (_receivedSize == 0)
                            {
                                Debug.LogTrace("Connection disconnected, received 0 size batch");
                                Disconnect();
                                return;
                            }

                            if (_receivedSize > _maxReceiveSize)
                            {
                                Debug.LogTrace("Connection disconnected, received buffer larger than max receive size {0}", _receivedSize);
                                Disconnect();
                                return;
                            }

                            _receiveArgs.SetBuffer(_receiveCache.Get(_receivedSize), 0, _receivedSize);
                        }
                        else
                        {
                            // payload received, queue buffer for processing
                            lock (_receiveLock)
                            {
                                _receiveQueueSize += _received;
                                if (_receiveQueueSize > _maxReceiveQueueSize)
                                {
                                    Debug.LogTrace("Connection disconnected, receive buffer queue size exceeded");
                                    Disconnect();
                                    return;
                                }
                                _receiveQueue.Enqueue(new ByteBuffer(args.Buffer, _received));
                            }

                            _receiveArgs.SetBuffer(_sizeBuffer, 0, 4);
                            _receivedSize = -1;
                        }
                        _received = 0;
                    }

                    var remaining = SizeToReceive - _received;
                    _receiveArgs.SetBuffer(_received, remaining);

                    try
                    {
                        if (_socket.ReceiveAsync(args))
                        {
                            return;
                        }
                    }
                    catch (ObjectDisposedException) { }
                    catch (Exception e)
                    {
                        Debug.LogError(e, $"An error occurred during {nameof(ProcessReceived)}");
                        Disconnect();
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e, "An unexpected receive error occurred");
                Disconnect();
            }
        }

        private void ProcessSent(object sender, SocketAsyncEventArgs args)
        {
            _sendCache.Return(args.Buffer);
            SendNext();
        }

        private void SendNext()
        {
            try
            {
                while (Connected)
                {
                    ByteBuffer buffer;
                    lock (_sendLock)
                    {
                        if (_sendQueue.Count == 0)
                        {
                            _sending = false;
                            return;
                        }
                        buffer = _sendQueue.Dequeue();
                    }

                    _sendArgs.SetBuffer(buffer.Data, 0, buffer.Count);

                    try
                    {
                        if (_socket.SendAsync(_sendArgs))
                        {
                            return;
                        }
                        _sendCache.Return(buffer.Data);
                    }
                    catch (ObjectDisposedException) { }
                    catch (Exception e)
                    {
                        Debug.LogError(e, $"An error occurred during {nameof(SendNext)}");
                        Disconnect();
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e, "An unexpected send error occurred");
                Disconnect();
            }
        }
    }
}
