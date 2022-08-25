namespace Zero.Game.Shared
{
    public class NetworkingOptions
    {
        public int MaxBufferSize { get; set; } = 10_000;

        public NetworkMode Mode { get; set; } = NetworkMode.Reliable;

        public int Port { get; set; } = 12_000;

        public int ReceiveTimeout { get; set; } = 5000;
    }
}
