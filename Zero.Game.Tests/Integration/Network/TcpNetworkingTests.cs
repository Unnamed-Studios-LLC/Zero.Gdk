using NUnit.Framework;
using System.Net;
using System.Threading.Tasks;
using Zero.Game.Common;
using Zero.Game.Server;

namespace Zero.Game.Tests.Integration.Network
{
    public class TcpNetworkingTests
    {
        [Test]
        public async Task Client_Connected_Test()
        {
            var key = "TEST_KEY".PadRight(20, ' ');
            var port = 24_000;

            var listener = CreateListener(port, key);
            bool receivedClient = false;
            var listenTask = Task.Run(async () =>
            {
                await foreach (var (keyResult, client) in listener.ReceiveClientAsync(default)
                    .ConfigureAwait(false))
                {
                    receivedClient = true;
                }
            });

            var client = new TcpNetworkClient(false);
            var connected = await client.ConnectAsync(IPAddress.Loopback.ToString(), port, key);

            await Task.Delay(100);

            listener.Stop();
            await listenTask;

            Assert.True(connected);
            Assert.True(receivedClient);
        }

        [Test]
        public async Task Client_Wrong_Key_Test()
        {
            var key = "TEST_KEY".PadRight(20, ' ');
            var wrongKey = "TEST_WRONG_KEY".PadRight(20, ' ');
            var port = 24_000;

            var listener = CreateListener(port, key);
            bool receivedClient = false;
            var listenTask = Task.Run(async () =>
            {
                await foreach (var (keyResult, client) in listener.ReceiveClientAsync(default))
                {
                    receivedClient = true;
                }
            });

            var client = new TcpNetworkClient(false);
            var connected = await client.ConnectAsync(IPAddress.Loopback.ToString(), port, wrongKey);

            await Task.Delay(100);

            listener.Stop();
            await listenTask;

            Assert.True(connected);
            Assert.False(receivedClient);
        }

        private TcpNetworkListener<object> CreateListener(int port, string key)
        {
            var listener = new TcpNetworkListener<object>();
            listener.Start(port, x => x == key ? new object() : null, default);
            return listener;
        }
    }
}
