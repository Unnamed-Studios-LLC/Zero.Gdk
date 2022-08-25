using System.Net;
using System.Threading.Tasks;

namespace Zero.Game.Common
{
    public interface INetworkClient
    {
        bool Connected { get; }

        int Latency { get; }

        int Port { get; }

        IPAddress RemoteIp { get; }

        void Close();

        Task<bool> ConnectAsync(string ip, int port, string key);

        Task<(bool, ByteBuffer)> ReceiveAsync();

        Task<(bool, string)> ReceiveKeyAsync();

        void SendReliable(ByteBuffer data);

        void SendUnreliable(ByteBuffer data);
    }
}
