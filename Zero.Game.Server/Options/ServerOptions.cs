using Zero.Game.Shared;

namespace Zero.Game.Server
{
    public class ServerOptions : GameOptions
    {
        public int ConnectionAcceptTimeoutMs { get; set; } = 10_000;

        public LogLevel LogLevel { get; set; } = LogLevel.Information;

        public int TickIntervalMs { get; set; } = 50;

        public uint UpdatesPerViewUpdate { get; set; } = 1;
    }
}
