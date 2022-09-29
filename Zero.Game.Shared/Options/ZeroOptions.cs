namespace Zero.Game.Shared
{
    public abstract class ZeroOptions
    {
        public InternalOptions InternalOptions { get; set; } = new InternalOptions();
        public LogLevel LogLevel { get; set; } = LogLevel.Information;
        public NetworkingOptions NetworkingOptions { get; set; } = new NetworkingOptions();
    }
}
