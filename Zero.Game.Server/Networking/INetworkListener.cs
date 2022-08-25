using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using Zero.Game.Common;

namespace Zero.Game.Server
{
    public interface INetworkListener<T>
        where T : class
    {
        IAsyncEnumerable<(T keyResult, INetworkClient client)> ReceiveClientAsync(CancellationToken token);

        void Start(int port, Func<string, T> keySelector, CancellationToken token);

        void Stop();

        void Whitelist(IPAddress address, DateTime timeoutUtc);
    }
}
