using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Zero.Game.Shared;

namespace Zero.Game.Common
{
    public class TcpNetworkClient : INetworkClient
    {
        private struct Packet
        {
            public TcpPacketType PacketType { get; }
            public ByteBuffer Buffer { get; }

            public Packet(TcpPacketType packetType, ByteBuffer buffer)
            {
                PacketType = packetType;
                Buffer = buffer;
            }
        }

        private bool _closed = false;
        private int _disposed = 0;
        private int _receivedCount = 0;
        private ByteBuffer _receiveBuffer;
        private ByteBuffer _sendBuffer;
        private bool _sending = false;
        private readonly Queue<Packet> _sendQueue = new Queue<Packet>();
        private readonly object _sendLock = new object();
        private readonly int _timeout = CommonDomain.Options?.Networking.ReceiveTimeout ?? 5000;

        private readonly ConcurrentDictionary<ushort, DateTime> _sentPings = new ConcurrentDictionary<ushort, DateTime>();
        private Timer _pingTimer;
        private ushort _nextPingId;

        private readonly Socket _socket;

        public bool Connected => _socket.Connected;
        public int Latency { get; private set; } = -1;
        public int Port { get; private set; }
        public IPAddress RemoteIp { get; private set; }

        public TcpNetworkClient(Socket socket)
        {
            _socket = socket;
            Port = ((IPEndPoint)_socket.LocalEndPoint).Port;
            RemoteIp = ((IPEndPoint)_socket.RemoteEndPoint).Address;
            if (RemoteIp.IsIPv4MappedToIPv6)
            {
                RemoteIp = RemoteIp.MapToIPv4();
            }

            Init();
            StartPingTimer(1_000);
        }

        public TcpNetworkClient(bool ipv6)
        {
            _socket = new Socket(ipv6 ? AddressFamily.InterNetworkV6 : AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            Init();
        }

        public void Close()
        {
            if (Interlocked.CompareExchange(ref _disposed, 1, 0) != 0)
            {
                return;
            }

            _closed = true;
            _pingTimer?.Dispose();
            _socket.Close();
        }

        public async Task<bool> ConnectAsync(string ip, int port, string key)
        {
            try
            {
                key = key.Substring(0, Math.Min(key.Length, 20)).PadRight(20);

                Port = port;
                RemoteIp = IPAddress.Parse(ip);
                await _socket.ConnectAsync(RemoteIp, port)
                    .ConfigureAwait(false);

                var buffer = PacketBufferCache.GetBuffer();
                var size = Encoding.ASCII.GetBytes(key, 0, key.Length, buffer.Data, 0);
                buffer = buffer.SetSize(size);

                if (!await SendBufferAsync(buffer)
                    .ConfigureAwait(false) ||
                    !_socket.Connected)
                {
                    return false;
                }

                StartPingTimer(100);
                return true;
            }
            catch (Exception e)
            {
                CommonDomain.PrivateLog(LogLevel.Error, e, "An error occurred during {0}", nameof(ConnectAsync));
                Close();
                return false;
            }
        }

        public async Task<(bool, ByteBuffer)> ReceiveAsync()
        {
            var receive = ReceiveBufferAsync();
            var timeout = Task.Delay(_timeout);

            var finished = await Task.WhenAny(receive, timeout)
                .ConfigureAwait(false);

            if (finished == timeout)
            {
                return (false, default);
            }

            return receive.Result;
        }

        public async Task<(bool, string)> ReceiveKeyAsync()
        {
            var receive = ReceiveKeyStringAsync();
            var timeout = Task.Delay(_timeout);

            var finished = await Task.WhenAny(receive, timeout)
                .ConfigureAwait(false);

            if (finished == timeout)
            {
                return (false, default);
            }

            return receive.Result;
        }

        public void SendReliable(ByteBuffer data)
        {
            SendPacket(new Packet(TcpPacketType.Payload, data));
        }

        public void SendUnreliable(ByteBuffer data)
        {
            SendReliable(data);
        }

        private void BeginRead(TaskCompletionSource<bool> completionSource)
        {
            if (_closed)
            {
                completionSource.TrySetResult(false);
                return;
            }

            try
            {
                _socket.BeginReceive(_receiveBuffer.Data, _receivedCount, _receiveBuffer.Size - _receivedCount, SocketFlags.None, out var socketError, EndRead, completionSource);

                if (IsCriticalSocketError(socketError))
                {
                    Close();
                    completionSource.TrySetResult(false);
                }
            }
            catch (Exception e)
            {
                CommonDomain.PrivateLog(LogLevel.Error, e, "An error occurred during {0}", nameof(BeginRead));
                completionSource.TrySetResult(false);
                return;
            }
        }

        private void BeginSend(ByteBuffer buffer, TaskCompletionSource<bool> completionSource)
        {
            if (_closed)
            {
                completionSource.TrySetResult(false);
                return;
            }

            try
            {
                _socket.BeginSend(buffer.Data, 0, buffer.Size, SocketFlags.None, out var socketError, EndSend, completionSource);
                if (IsCriticalSocketError(socketError))
                {
                    Close();
                    completionSource.TrySetResult(false);
                }
            }
            catch (Exception e)
            {
                CommonDomain.PrivateLog(LogLevel.Error, e, "An error occurred during {0}", nameof(BeginSend));
                completionSource.TrySetResult(false);
                return;
            }
        }

        private void EndRead(IAsyncResult ar)
        {
            var completionSource = (TaskCompletionSource<bool>)ar.AsyncState;
            if (_closed)
            {
                completionSource.TrySetResult(false);
                return;
            }

            try
            {
                var read = _socket.EndReceive(ar, out var socketError);
                if (read <= 0 ||
                    IsCriticalSocketError(socketError))
                {
                    Close();
                    completionSource.TrySetResult(false);
                    return;
                }

                _receivedCount += read;
                if (_receivedCount < _receiveBuffer.Size)
                {
                    BeginRead(completionSource);
                    return;
                }
                completionSource.TrySetResult(true);
            }
            catch (Exception e)
            {
                CommonDomain.PrivateLog(LogLevel.Error, e, "An error occurred during {0}", nameof(EndRead));
                Close();
                completionSource.TrySetResult(false);
            }
        }

        private void EndSend(IAsyncResult ar)
        {
            var completionSource = (TaskCompletionSource<bool>)ar.AsyncState;
            if (_closed)
            {
                completionSource.TrySetResult(false);
                return;
            }

            try
            {
                var sent = _socket.EndSend(ar, out var socketError);
                if (IsCriticalSocketError(socketError))
                {
                    Close();
                    completionSource.TrySetResult(false);
                    return;
                }

                completionSource.TrySetResult(true);
            }
            catch (Exception e)
            {
                CommonDomain.PrivateLog(LogLevel.Error, e, "An error occurred during {0}", nameof(EndSend));
                Close();
                completionSource.TrySetResult(false);
            }
        }

        private ByteBuffer GetNextPing()
        {
            var now = DateTime.UtcNow;
            var pingId = _nextPingId++;

            var buffer = PacketBufferCache.GetBuffer();
            var writer = new BitWriter(buffer.Data);
            writer.Write(pingId);
            buffer = writer.GetBuffer();

            _sentPings[pingId] = now;

            return buffer;
        }

        private bool IsCriticalSocketError(SocketError error)
        {
            switch (error)
            {
                case SocketError.Success:
                case SocketError.IOPending:
                case SocketError.WouldBlock:
                    return false;
                default:
                    return true;
            }
        }

        private void Init()
        {
            _socket.SendBufferSize = PacketBufferCache.MaxBufferSize * 2;
            _socket.ReceiveBufferSize = PacketBufferCache.MaxBufferSize * 2;
            _socket.NoDelay = true;
        }

        private void OnTimer(object state)
        {
            var ping = GetNextPing();
            SendPacket(new Packet(TcpPacketType.Ping, ping));

            var now = DateTime.UtcNow;
            foreach (var sentPair in _sentPings)
            {
                var difference = (now - sentPair.Value).TotalSeconds;
                if (difference > _timeout)
                {
                    Close();
                }
            }
        }

        private async Task<(bool, ByteBuffer)> ReceiveBufferAsync()
        {
            TcpPacketType packetType;
            while (_socket.Connected)
            {
                _receiveBuffer = PacketBufferCache.GetBuffer();

                // read size value
                if (!await ReceiveBytesAsync(5)
                    .ConfigureAwait(false))
                {
                    PacketBufferCache.ReturnBuffer(_receiveBuffer);
                    return (false, default);
                }

                var size = (_receiveBuffer.Data[0] << 24) |
                    (_receiveBuffer.Data[1] << 16) |
                    (_receiveBuffer.Data[2] << 8) |
                    (_receiveBuffer.Data[3]);

                packetType = (TcpPacketType)_receiveBuffer.Data[4];

                // read packet data
                if (!await ReceiveBytesAsync(size)
                    .ConfigureAwait(false))
                {
                    PacketBufferCache.ReturnBuffer(_receiveBuffer);
                    return (false, default);
                }

                switch (packetType)
                {
                    case TcpPacketType.Ping:
                        ReceivedPing(_receiveBuffer);
                        continue;
                    case TcpPacketType.Pong:
                        ReceivedPong(_receiveBuffer);
                        continue;
                    default:
                        return (true, _receiveBuffer);
                }
            }
            return (false, default);
        }

        private async Task<bool> ReceiveBytesAsync(int size)
        {
            ResetBufferSize(size);
            var completionSource = new TaskCompletionSource<bool>();
            BeginRead(completionSource);
            return await completionSource.Task.ConfigureAwait(false);
        }

        private async Task<(bool, string)> ReceiveKeyStringAsync()
        {
            _receiveBuffer = PacketBufferCache.GetBuffer();

            if (!await ReceiveBytesAsync(20).ConfigureAwait(false))
            {
                PacketBufferCache.ReturnBuffer(_receiveBuffer);
                return (false, default);
            }

            var key = Encoding.ASCII.GetString(_receiveBuffer.Data, 0, _receiveBuffer.Size);
            PacketBufferCache.ReturnBuffer(_receiveBuffer);

            return (true, key);
        }

        private void ReceivedPing(ByteBuffer buffer)
        {
            SendPacket(new Packet(TcpPacketType.Pong, buffer.SetSize(2)));
        }

        private void ReceivedPong(ByteBuffer buffer)
        {
            var reader = new BitReader(buffer.Data, 0, buffer.Size);
            var id = reader.ReadUInt16();
            PacketBufferCache.ReturnBuffer(buffer);

            if (!_sentPings.TryRemove(id, out var sentTime))
            {
                return;
            }

            var now = DateTime.UtcNow;
            Latency = (int)(now - sentTime).TotalMilliseconds;
        }

        private void ResetBufferSize(int size)
        {
            _receivedCount = 0;
            _receiveBuffer = _receiveBuffer.SetSize(size);
        }

        private async Task SendLoopAsync()
        {
            _sendBuffer = PacketBufferCache.GetBuffer();

            try
            {
                while (Connected)
                {
                    Packet packet;
                    lock (_sendLock)
                    {
                        if (_sendQueue.Count == 0)
                        {
                            _sending = false;
                            return;
                        }
                        packet = _sendQueue.Dequeue();
                    }

                    var size = packet.Buffer.Size;
                    _sendBuffer = _sendBuffer.SetSize(size + 5);
                    _sendBuffer.Data[0] = (byte)(size >> 24);
                    _sendBuffer.Data[1] = (byte)(size >> 16);
                    _sendBuffer.Data[2] = (byte)(size >> 8);
                    _sendBuffer.Data[3] = (byte)size;
                    _sendBuffer.Data[4] = (byte)packet.PacketType;

                    Buffer.BlockCopy(packet.Buffer.Data, 0, _sendBuffer.Data, 5, size);

                    try
                    {
                        if (!await SendBufferAsync(_sendBuffer).ConfigureAwait(false))
                        {
                            Close();
                            return;
                        }
                    }
                    finally
                    {
                        PacketBufferCache.ReturnBuffer(packet.Buffer);
                    }
                }
            }
            catch (Exception e)
            {
                CommonDomain.PrivateLog(LogLevel.Error, e, "An error occurred during {0}", nameof(SendLoopAsync));
                Close();
            }
            finally
            {
                PacketBufferCache.ReturnBuffer(_sendBuffer);
            }
        }

        private async Task<bool> SendBufferAsync(ByteBuffer buffer)
        {
            var completionSource = new TaskCompletionSource<bool>();
            BeginSend(buffer, completionSource);
            return await completionSource.Task.ConfigureAwait(false);
        }

        private void SendPacket(Packet packet)
        {
            lock (_sendLock)
            {
                _sendQueue.Enqueue(packet);
                if (_sending)
                {
                    return;
                }
                _sending = true;
            }
            _ = SendLoopAsync();
        }

        private void StartPingTimer(int delay)
        {
            _pingTimer = new Timer(OnTimer, null, delay, 1_000);
        }
    }
}
