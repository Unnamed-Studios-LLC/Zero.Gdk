using System.Net;
using System.Threading.Tasks;

namespace Zero.Game.Common
{
    public class UdpNetworkClient : INetworkClient
    {
        public bool Connected => throw new System.NotImplementedException();

        public int Latency => throw new System.NotImplementedException();

        public int Port => throw new System.NotImplementedException();

        public void Close()
        {
            throw new System.NotImplementedException();
        }

        public IPAddress RemoteIp => throw new System.NotImplementedException();

        public Task<bool> ConnectAsync(string ip, int port, string key)
        {
            throw new System.NotImplementedException();
        }

        public Task<(bool, ByteBuffer)> ReceiveAsync()
        {
            throw new System.NotImplementedException();
        }

        public Task<(bool, string)> ReceiveKeyAsync()
        {
            throw new System.NotImplementedException();
        }

        public void SendReliable(ByteBuffer data)
        {
            throw new System.NotImplementedException();
        }

        public void SendUnreliable(ByteBuffer data)
        {
            throw new System.NotImplementedException();
        }
    }
}
