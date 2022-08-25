using System;
using Zero.Game.Shared;

namespace Zero.Game.Common
{
    internal static class CommonDomain
    {
        private const string InternalLogPrefix = "[Internal] ";
        private const string PrivateLogPrefix = "[Private] ";

        public static ILoggingProvider LoggingProvider { get; set; }
        public static GameOptions Options { get; set; }
        public static LogLevel PrivateLogLevel { get; set; }
        public static CommonSchema Schema { get; set; }

        internal static void InternalLog(LogLevel level, string message)
        {
            InternalLog(level, null, message);
        }

        internal static void InternalLog(LogLevel level, Exception e, string message)
        {
            if (level < Options.InternalLogLevel)
            {
                return;
            }

            LoggingProvider?.Log(level, $"{InternalLogPrefix}{message}", e);
        }

        internal static void InternalLog(LogLevel level, string format, params object[] args)
        {
            InternalLog(level, null, format, args);
        }

        internal static void InternalLog(LogLevel level, Exception e, string format, params object[] args)
        {
            if (level < Options.InternalLogLevel)
            {
                return;
            }

            LoggingProvider?.Log(level, $"{InternalLogPrefix}{format}", args, e);
        }

        internal static void PrivateLog(LogLevel level, string message)
        {
            PrivateLog(level, null, message);
        }

        internal static void PrivateLog(LogLevel level, Exception e, string message)
        {
            if (level < PrivateLogLevel)
            {
                return;
            }

            LoggingProvider?.Log(level, $"{PrivateLogPrefix}{message}", e);
        }

        internal static void PrivateLog(LogLevel level, string format, params object[] args)
        {
            PrivateLog(level, null, format, args);
        }

        internal static void PrivateLog(LogLevel level, Exception e, string format, params object[] args)
        {
            if (level < PrivateLogLevel)
            {
                return;
            }

            LoggingProvider?.Log(level, $"{PrivateLogPrefix}{format}", args, e);
        }
    }
}
