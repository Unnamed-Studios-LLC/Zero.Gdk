using Zero.Game.Shared;

namespace Zero.Game.Server
{
    public class ServerOptions : ZeroOptions
    {
        public int ConnectionAcceptTimeoutMs { get; set; } = 10_000;
        public int GracefulStopTimeoutMs { get; set; } = 10_000;
        public bool LogHeader { get; set; } = true;
        public int Port { get; set; } = 12_000;
        public int MaxConnections { get; set; } = 200;
        public int UpdateIntervalMs { get; set; } = 50;
        public uint UpdatesPerViewUpdate { get; set; } = 5;
    }
}
