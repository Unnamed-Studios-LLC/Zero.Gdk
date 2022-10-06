using Zero.Game.Shared;

namespace Zero.Game.Server
{
    public class ServerOptions : ZeroOptions
    {
        /// <summary>
        /// The amount of ms to keep a connection key alive for
        /// </summary>
        public int ConnectionAcceptTimeoutMs { get; set; } = 10_000;
        /// <summary>
        /// The amount of ms to allow the server to stop gracefully
        /// </summary>
        public int GracefulStopTimeoutMs { get; set; } = 10_000;
        /// <summary>
        /// If the Zero header should be output
        /// </summary>
        public bool LogHeader { get; set; } = true;
        /// <summary>
        /// The port to communicate on
        /// </summary>
        public int Port { get; set; } = 12_000;
        /// <summary>
        /// The maximum amount of connection to allow simultaneously, default is no max (-1)
        /// </summary>
        public int MaxConnections { get; set; } = -1;
        /// <summary>
        /// The target update delta in ms
        /// </summary>
        public int UpdateIntervalMs { get; set; } = 50;
        /// <summary>
        /// The amount of updates to occur before view queries are called
        /// </summary>
        public uint UpdatesPerViewUpdate { get; set; } = 5;
    }
}
