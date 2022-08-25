using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Zero.Game.Common;

namespace Zero.Game.Server
{
    public class TcpNetworkListener<T> : INetworkListener<T>
        where T : class
    {
        private const float TrimInterval = 10;

        private TcpListener _listener;
        private Task _receiveTask;
        private Func<string, T> _keySelector;
        private bool _stopped = true;
        private TaskCompletionSource<bool> _receiveSource = new TaskCompletionSource<bool>();
        private readonly ConcurrentQueue<(T, TcpNetworkClient)> _receivedClients = new ConcurrentQueue<(T, TcpNetworkClient)>();
        private readonly ConcurrentDictionary<IPAddress, DateTime> _whitelist = new ConcurrentDictionary<IPAddress, DateTime>();
        private DateTime _nextWhitelistTrim = DateTime.UtcNow;

        public async IAsyncEnumerable<(T keyResult, INetworkClient client)> ReceiveClientAsync([EnumeratorCancellation] CancellationToken token)
        {
            while (!token.IsCancellationRequested && 
                await _receiveSource.Task.ConfigureAwait(false) && 
                !_stopped)
            {
                _receiveSource = new TaskCompletionSource<bool>();
                while (_receivedClients.TryDequeue(out var client))
                {
                    yield return client;
                }
            }
            yield break;
        }

        public void Start(int port, Func<string, T> keySelector, CancellationToken token)
        {
            _stopped = false;
            _keySelector = keySelector;
            _listener = new TcpListener(IPAddress.IPv6Any, port);
            _listener.Server.DualMode = true;
            _listener.Server.SetSocketOption(SocketOptionLevel.IPv6, SocketOptionName.IPv6Only, false);
            _listener.Start();
            _receiveTask = ReceiveLoopAsync(token);
        }

        public void Stop()
        {
            _stopped = true;
            _listener.Stop();
            _receiveSource.TrySetResult(false);

            try
            {
                _receiveTask.GetAwaiter().GetResult();
            }
            catch { }
        }

        public void Whitelist(IPAddress address, DateTime timeoutUtc)
        {
            _whitelist[address] = timeoutUtc;
        }

        private async Task ReceiveClientAsync(Socket socket)
        {
            var remoteAddress = (socket.RemoteEndPoint as IPEndPoint).Address;
            if (remoteAddress.IsIPv4MappedToIPv6)
            {
                remoteAddress = remoteAddress.MapToIPv4();
            }

            if (!_whitelist.TryGetValue(remoteAddress, out var timeout) ||
                timeout < DateTime.UtcNow)
            {
                //ServerDomain.PrivateLog(Shared.LogLevel.Information, "Address not whitelisted {0} {1}", remoteAddress, timeout);
                socket.Dispose();
                return;
            }

            var networkClient = new TcpNetworkClient(socket);
            var (success, key) = await networkClient.ReceiveKeyAsync()
                .ConfigureAwait(false);
            if (!success)
            {
                //ServerDomain.PrivateLog(Shared.LogLevel.Information, "Key not valid");
                socket.Dispose();
                return;
            }

            var data = _keySelector(key);
            if (data == null)
            {
                //ServerDomain.PrivateLog(Shared.LogLevel.Information, "No data for the received key");
                socket.Dispose();
                return;
            }

            _receivedClients.Enqueue((data, networkClient));
            _receiveSource.TrySetResult(true);
        }

        private async Task ReceiveLoopAsync(CancellationToken token)
        {
            while (!token.IsCancellationRequested && !_stopped)
            {
                var socket = await _listener.AcceptSocketAsync()
                    .ConfigureAwait(false);

                _ = ReceiveClientAsync(socket);

                if (DateTime.UtcNow >= _nextWhitelistTrim)
                {
                    TrimWhitelist();
                    _nextWhitelistTrim = DateTime.UtcNow.AddSeconds(TrimInterval);
                }
            }
        }

        private void TrimWhitelist()
        {
            var now = DateTime.UtcNow;
            foreach (var pair in _whitelist)
            {
                if (pair.Value >= now)
                {
                    continue;
                }
                _whitelist.TryRemove(pair.Key, out _);
            }
        }
    }
}
