using System;
using System.Runtime.CompilerServices;

namespace Zero.Game.Shared
{
    public static class Debug
    {
        public static void Log(LogLevel level, string message, [CallerMemberName] string callerMemberName = null, [CallerFilePath] string callerFilePath = null, [CallerLineNumber] int callerLineNumber = 0)
        {
            Log(level, null, message);
        }

        public static void Log(LogLevel level, Exception e, string message)
        {
            if (level < SharedDomain.LogLevel)
            {
                return;
            }

            SharedDomain.LoggingProvider.Log(level, message, e);
        }

        public static void Log(LogLevel level, string format, params object[] args)
        {
            Log(level, null, format, args);
        }

        public static void Log(LogLevel level, Exception e, string format, params object[] args)
        {
            if (level < SharedDomain.LogLevel)
            {
                return;
            }

            SharedDomain.LoggingProvider.Log(level, format, args, e);
        }

        public static void Log(object objValue)
        {
            Log(LogLevel.Information, objValue?.ToString() ?? "(null)");
        }

        public static void Log(string message)
        {
            Log(LogLevel.Information, message);
        }

        public static void Log(string format, params object[] args)
        {
            Log(LogLevel.Information, format, args);
        }

        public static void Log(Exception e, string message)
        {
            Log(LogLevel.Information, e, message);
        }

        public static void Log(Exception e, string format, params object[] args)
        {
            Log(LogLevel.Information, e, format, args);
        }

        public static void LogCritical(string message)
        {
            Log(LogLevel.Critical, message);
        }

        public static void LogCritical(string format, params object[] args)
        {
            Log(LogLevel.Critical, format, args);
        }

        public static void LogCritical(Exception e, string message)
        {
            Log(LogLevel.Critical, e, message);
        }

        public static void LogCritical(Exception e, string format, params object[] args)
        {
            Log(LogLevel.Critical, e, format, args);
        }

        public static void LogDebug(string message)
        {
            Log(LogLevel.Debug, message);
        }

        public static void LogDebug(string format, params object[] args)
        {
            Log(LogLevel.Debug, format, args);
        }

        public static void LogError(string message)
        {
            Log(LogLevel.Error, message);
        }

        public static void LogError(string format, params object[] args)
        {
            Log(LogLevel.Error, format, args);
        }

        public static void LogError(Exception e, string message)
        {
            Log(LogLevel.Error, e, message);
        }

        public static void LogError(Exception e, string format, params object[] args)
        {
            Log(LogLevel.Error, e, format, args);
        }

        public static void LogInfo(string message)
        {
            Log(LogLevel.Information, message);
        }

        public static void LogInfo(string format, params object[] args)
        {
            Log(LogLevel.Information, format, args);
        }

        public static void LogTrace(string message)
        {
            Log(LogLevel.Trace, message);
        }

        public static void LogTrace(string format, params object[] args)
        {
            Log(LogLevel.Trace, format, args);
        }

        public static void LogWarning(string message)
        {
            Log(LogLevel.Warning, message);
        }

        public static void LogWarning(string format, params object[] args)
        {
            Log(LogLevel.Warning, format, args);
        }
    }
}
