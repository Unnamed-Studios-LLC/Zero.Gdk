using System;
using Zero.Game.Common;
using Zero.Game.Shared;

namespace Zero.Game.Server
{
    internal static class ServerDomain
    {
        private const string InternalLogPrefix = "[Internal] ";
        private const string PrivateLogPrefix = "[Private] ";

        public static IDeploymentProvider DeploymentProvider { get; set; }

        public static ILoggingProvider LoggingProvider { get; set; }

        public static ServerOptions Options { get; set; }

        public static LogLevel PrivateLogLevel { get; set; }

        public static ServerSchema Schema { get; set; }

        public static ServerSetup Setup { get; set; }

        public static INetworkListener<StartConnectionRequest> CreateNetworkListener()
        {
            return Options.Networking.Mode switch
            {
                NetworkMode.Unreliable => new UdpNetworkListener<StartConnectionRequest>(),
                _ => new TcpNetworkListener<StartConnectionRequest>(),
            };
        }

        public static Node CreateNode()
        {
            return new Node(CreateNetworkListener());
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
