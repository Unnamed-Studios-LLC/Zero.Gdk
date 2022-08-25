namespace Zero.Game.Shared
{
    public abstract class GameOptions
    {
        public LogLevel InternalLogLevel { get; set; } = LogLevel.Error;

        public NetworkingOptions Networking { get; set; } = new NetworkingOptions();
    }
}
