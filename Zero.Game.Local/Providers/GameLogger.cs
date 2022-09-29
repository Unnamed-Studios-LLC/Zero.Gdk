using System;
using Microsoft.Extensions.Logging;
using Zero.Game.Server;
using Zero.Game.Shared;

namespace Zero.Game.Local.Providers
{
    public class GameLogger : ILoggingProvider
    {
        private readonly ILogger<ZeroServer> _logger;

        public GameLogger(ILogger<ZeroServer> logger)
        {
            _logger = logger;
        }

        public void Log(Shared.LogLevel level, string message, Exception e)
        {
            switch (level)
            {
                case Shared.LogLevel.Trace:
                    _logger.LogTrace(e, message);
                    break;
                case Shared.LogLevel.Debug:
                    _logger.LogDebug(e, message);
                    break;
                case Shared.LogLevel.Information:
                    _logger.LogInformation(e, message);
                    break;
                case Shared.LogLevel.Warning:
                    _logger.LogWarning(e, message);
                    break;
                case Shared.LogLevel.Error:
                    _logger.LogError(e, message);
                    break;
                case Shared.LogLevel.Critical:
                    _logger.LogCritical(e, message);
                    break;
            }
        }

        public void Log(Shared.LogLevel level, string format, object[] args, Exception e)
        {
            switch (level)
            {
                case Shared.LogLevel.Trace:
                    _logger.LogTrace(e, format, args);
                    break;
                case Shared.LogLevel.Debug:
                    _logger.LogDebug(e, format, args);
                    break;
                case Shared.LogLevel.Information:
                    _logger.LogInformation(e, format, args);
                    break;
                case Shared.LogLevel.Warning:
                    _logger.LogWarning(e, format, args);
                    break;
                case Shared.LogLevel.Error:
                    _logger.LogError(e, format, args);
                    break;
                case Shared.LogLevel.Critical:
                    _logger.LogCritical(e, format, args);
                    break;
            }
        }
    }
}
