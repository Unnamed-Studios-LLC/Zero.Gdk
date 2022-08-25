using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using Zero.Game.Common;

namespace Zero.Game.Server
{
    public class UdpNetworkListener<T> : INetworkListener<T>
        where T : class
    {
        public IAsyncEnumerable<(T keyResult, INetworkClient client)> ReceiveClientAsync(CancellationToken token)
        {
            throw new NotImplementedException();
        }

        public void Start(int port, Func<string, T> keySelector, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        public void Stop()
        {
            throw new NotImplementedException();
        }
        public void Whitelist(IPAddress address, DateTime timeoutUtc)
        {
            throw new NotImplementedException();
        }
    }
}
