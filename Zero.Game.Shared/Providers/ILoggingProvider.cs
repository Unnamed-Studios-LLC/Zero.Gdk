using System;

namespace Zero.Game.Shared
{
    public interface ILoggingProvider
    {
        void Log(LogLevel logLevel, string message, Exception e);

        void Log(LogLevel logLevel, string format, object[] args, Exception e);
    }
}
