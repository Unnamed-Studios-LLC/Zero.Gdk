using System;
using Zero.Game.Common;
using Zero.Game.Shared;

namespace Zero.Game.Client
{
    internal static class ClientDomain
    {
        private const string InternalLogPrefix = "[Internal] ";
        private const string PrivateLogPrefix = "[Private] ";
        private const LogLevel PrivateLogLevel = LogLevel.Trace;

        public static ILoggingProvider LoggingProvider { get; set; }
        public static ClientOptions Options { get; set; }
        public static ClientSchema Schema { get; set; }
        public static ClientSetup Setup { get; set; }

        public static INetworkClient CreateNetworkClient(bool ipv6)
        {
            switch (Options.Networking.Mode)
            {
                default:
                    return new TcpNetworkClient(ipv6);
            }
        }

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

            LoggingProvider.Log(level, $"{InternalLogPrefix}{message}", e);
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

            LoggingProvider.Log(level, $"{InternalLogPrefix}{format}", args, e);
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

            LoggingProvider.Log(level, $"{PrivateLogPrefix}{message}", e);
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

            LoggingProvider.Log(level, $"{PrivateLogPrefix}{format}", args, e);
        }
    }
}
