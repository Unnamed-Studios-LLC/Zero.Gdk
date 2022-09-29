using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Zero.Game.Shared;

namespace Zero.Game.Server
{
    public class ConnectionValidator<TState>
    {
        private class Bucket
        {
            public DateTime Time { get; set; }
            public int Count { get; set; }
        }

        private const int KeyLength = 20;

        private readonly ConcurrentDictionary<IPAddress, Bucket> _rateBuckets = new();
        private readonly ConcurrentDictionary<string, TState> _keyMap = new();
        private readonly Stack<SocketAsyncEventArgs> _receiveArgs = new();
        private readonly Queue<(Socket, TState)> _validatedSockets = new();
        private readonly Queue<(string, DateTime)> _expirationQueue = new();
        private readonly TimeSpan _keyLifetime;
        private readonly int _requestsPerIpPerPeriod;
        private readonly TimeSpan _perIpPeriod = TimeSpan.FromSeconds(10);
        private int _stopped;

        public ConnectionValidator(TimeSpan keyLifetime, int requestsPerIpPerPeriod)
        {
            _keyLifetime = keyLifetime;
            _requestsPerIpPerPeriod = requestsPerIpPerPeriod;
        }

        private bool Stopped => _stopped != 0;

        public void GetValidated(List<(Socket, TState)> list)
        {
            lock (_validatedSockets)
            {
                while (_validatedSockets.TryDequeue(out var socketPair))
                {
                    list.Add(socketPair);
                }
            }

            var now = DateTime.UtcNow;
            lock (_expirationQueue)
            {
                while (_expirationQueue.TryPeek(out var pair))
                {
                    if (pair.Item2 > now)
                    {
                        break;
                    }

                    _keyMap.TryRemove(pair.Item1, out _);
                    _expirationQueue.Dequeue();
                }
            }
        }

        public StartConnectionResponse OpenConnection(IPAddress ipAddress, TState state)
        {
            if (ExceededRate(ipAddress))
            {
                return ConnectionFailReason.PerIpRateExceeded;
            }

            string key;
            do
            {
                key = GenerateKey();
            }
            while (!_keyMap.TryAdd(key, state));
            var expiration = (key, DateTime.UtcNow + _keyLifetime);
            lock (_expirationQueue)
            {
                _expirationQueue.Enqueue(expiration);
            }
            return new StartConnectionResponse(string.Empty, 0, key);
        }

        public void StartValidation(Socket socket)
        {
            var args = GetArgs();
            args.UserToken = socket;
            args.SetBuffer(0, KeyLength);

            try
            {
                if (!socket.ReceiveAsync(args))
                {
                    ThreadPool.QueueUserWorkItem(x => ProcessReceived(null, (SocketAsyncEventArgs)x), args);
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e, $"An error occurred during {nameof(StartValidation)}");
                socket.Close();
            }
        }

        public void StopAll()
        {
            if (Interlocked.CompareExchange(ref _stopped, 1, 0) == 1)
            {
                return;
            }
        }

        private bool ExceededRate(IPAddress ipAddress)
        {
            var now = DateTime.UtcNow;
            Bucket bucket;
            do
            {
                if (_rateBuckets.TryGetValue(ipAddress, out bucket))
                {
                    break;
                }

                bucket = new Bucket
                {
                    Time = now,
                    Count = 0
                };
            }
            while (!_rateBuckets.TryAdd(ipAddress, bucket));

            lock (bucket)
            {
                if (now - bucket.Time > _perIpPeriod)
                {
                    bucket.Time = now;
                    bucket.Count = 0;
                }
                else
                {
                    bucket.Count++;
                }

                return bucket.Count >= _requestsPerIpPerPeriod;
            }
        }

        private static string GenerateKey()
        {
            return Random.StringAlphaNumeric(KeyLength);
        }

        private SocketAsyncEventArgs GetArgs()
        {
            SocketAsyncEventArgs args;
            lock (_receiveArgs)
            {
                if (_receiveArgs.TryPop(out args))
                {
                    args.SetBuffer(0, KeyLength);
                    return args;
                }
            }

            args = new SocketAsyncEventArgs();
            args.Completed += ProcessReceived;
            args.SetBuffer(new byte[KeyLength], 0, KeyLength);
            return args;
        }

        private void ProcessReceived(object sender, SocketAsyncEventArgs args)
        {
            var socket = (Socket)args.UserToken;
            if (Stopped ||
                args.SocketError != SocketError.Success)
            {
                socket.Shutdown(SocketShutdown.Both);
                socket.Close();
                ReturnArgs(args);
                return;
            }

            while (!Stopped && socket.Connected)
            {
                var received = args.Offset + args.BytesTransferred;
                var remaining = KeyLength - received;
                if (remaining == 0)
                {
                    var key = Encoding.ASCII.GetString(args.Buffer.AsSpan());
                    if (_keyMap.TryRemove(key, out var state))
                    {
                        // validated, queue request for connection creation
                        lock (_validatedSockets)
                        {
                            _validatedSockets.Enqueue((socket, state));
                        }
                    }
                    else
                    {
                        socket.Shutdown(SocketShutdown.Both);
                        socket.Close();
                    }

                    ReturnArgs(args);
                    return;
                }

                args.SetBuffer(received, remaining);
                try
                {
                    if (socket.ReceiveAsync(args))
                    {
                        return;
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError(e, $"An error occurred during {nameof(ProcessReceived)}");
                    // TODO disconnect
                }
            }
        }

        private void ReturnArgs(SocketAsyncEventArgs args)
        {
            lock (_receiveArgs)
            {
                _receiveArgs.Push(args);
            }
        }
    }
}
