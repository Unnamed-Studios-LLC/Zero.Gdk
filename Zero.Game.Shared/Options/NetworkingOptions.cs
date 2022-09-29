namespace Zero.Game.Shared
{
    public class NetworkingOptions
    {
        public int ClientBufferSize { get; set; } = 10_000;
        public int ClientMaxReceiveQueueSize { get; set; } = 20_000;
        public int ServerBufferSize { get; set; } = 50_000;
        public int ServerMaxReceiveQueueSize { get; set; } = 50_000;
        public NetworkMode Mode { get; set; } = NetworkMode.Reliable;
        public int ReceiveTimeoutMs { get; set; } = 5_000;
    }
}
