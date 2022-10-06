namespace Zero.Game.Shared
{
    public abstract class ZeroOptions
    {
        public InternalOptions InternalOptions { get; } = new InternalOptions();
        /// <summary>
        /// The minimum log level for console files
        /// </summary>
        public LogLevel LogLevel { get; set; } = LogLevel.Information;
        public NetworkingOptions NetworkingOptions { get; } = new NetworkingOptions();
    }
}
