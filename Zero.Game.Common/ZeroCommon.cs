using Zero.Game.Shared;

namespace Zero.Game.Common
{
    public static class ZeroCommon
    {
        public static void Setup(ILoggingProvider loggingProvider, GameOptions options, LogLevel privateLogLevel, CommonSchema schema)
        {
            PacketBufferCache.MaxBufferSize = options.Networking.MaxBufferSize;
            CommonDomain.LoggingProvider = loggingProvider;
            CommonDomain.Options = options;
            CommonDomain.PrivateLogLevel = privateLogLevel;
            CommonDomain.Schema = schema;
        }
    }
}
