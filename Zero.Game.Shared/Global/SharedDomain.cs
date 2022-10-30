using System;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Zero.Game.Benchmark")]
[assembly: InternalsVisibleTo("Zero.Game.Tests")]
[assembly: InternalsVisibleTo("Zero.Game.Client")]
[assembly: InternalsVisibleTo("Zero.Game.Server")]

namespace Zero.Game.Shared
{
    internal static class SharedDomain
    {
        private const string InternalLogPrefix = "[Internal] ";

        public static ILoggingProvider LoggingProvider { get; private set; }
        public static LogLevel LogLevel { get; private set; }
        public static LogLevel InternalLogLevel { get; private set; }
        public static DataDefinition[] DataDefinitions { get; private set; }
        public static long DataHash { get; private set; }

        internal static void InternalLog(LogLevel level, string message)
        {
            InternalLog(level, null, message);
        }

        internal static void InternalLog(LogLevel level, Exception e, string message)
        {
            if (level < InternalLogLevel)
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
            if (level < InternalLogLevel)
            {
                return;
            }

            LoggingProvider?.Log(level, $"{InternalLogPrefix}{format}", args, e);
        }

        internal static void SetLogger(ILoggingProvider loggingProvider, LogLevel logLevel)
        {
            LoggingProvider = loggingProvider;
            LogLevel = logLevel;
        }

        internal static void Setup(InternalOptions options, DataDefinition[] dataDefinitions)
        {
            InternalLogLevel = options.InternalLogLevel;
            DataDefinitions = dataDefinitions;
            DataHash = GenerateDataHash(dataDefinitions);
        }

        private static long GenerateDataHash(DataDefinition[] dataDefinitions)
        {
            long hash = 1;
            foreach (var dataDefinition in dataDefinitions) dataDefinition.ApplyHash(ref hash);
            return hash;
        }
    }
}
