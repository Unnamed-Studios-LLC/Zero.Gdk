namespace Zero.Game.Shared
{
    public class NetworkingOptions
    {
        /// <summary>
        /// The client's buffer size in bytes for client send and server receive
        /// </summary>
        public int ClientBufferSize { get; set; } = 10_000;
        /// <summary>
        /// The maximum amount of bytes per connection that can be in queue for client receiving
        /// </summary>
        public int ClientMaxReceiveQueueSize { get; set; } = 20_000;
        /// <summary>
        /// The server's buffer size in bytes for server send and client receive
        /// </summary>
        public int ServerBufferSize { get; set; } = 50_000;
        /// <summary>
        /// The maximum amount of bytes per connection that can be in queue for server receiving
        /// </summary>
        public int ServerMaxReceiveQueueSize { get; set; } = 50_000;
        /// <summary>
        /// The current networking mode. Only reliable in support at the moment.
        /// </summary>
        public NetworkMode Mode { get; set; } = NetworkMode.Reliable;
        /// <summary>
        /// The amount of ms to wait before timing out a connection that isn't receiving data
        /// </summary>
        public int ReceiveTimeoutMs { get; set; } = 5_000;
    }
}
