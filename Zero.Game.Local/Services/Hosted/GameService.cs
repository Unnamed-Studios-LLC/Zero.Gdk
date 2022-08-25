using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Zero.Game.Server;
using Zero.Game.Shared;

namespace Zero.Game.Local.Services.Hosted
{
    public class GameService : IHostedService
    {
        private Thread _executionThread;
        private readonly TaskCompletionSource<bool> _stopped = new();

        private readonly ILogger<GameService> _logger;
        private readonly IHostApplicationLifetime _lifetime;
        private readonly ServerSetup _serverSetup;
        private readonly IDeploymentProvider _deploymentProvider;
        private readonly ILoggingProvider _loggingProvider;

        public GameService(ILogger<GameService> logger,
            IHostApplicationLifetime lifetime,
            ServerSetup serverSetup,
            IDeploymentProvider deploymentProvider,
            ILoggingProvider loggingProvider)
        {
            _logger = logger;
            _lifetime = lifetime;
            _serverSetup = serverSetup;
            _deploymentProvider = deploymentProvider;
            _loggingProvider = loggingProvider;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            if (_serverSetup == null)
            {
                _logger.LogError("Failed to start deployment. Unable to load valid build");
                Environment.ExitCode = 1;
                _lifetime.StopApplication();
                return;
            }

            ZeroServer.Setup(_serverSetup, _deploymentProvider, _loggingProvider);

            Debug.LogInfo("Starting server...");

            await _serverSetup.StartWorkerAsync()
                .ConfigureAwait(false);
            await _serverSetup.StartDeploymentAsync()
                .ConfigureAwait(false);

            Debug.LogInfo("Press Ctrl+C to stop");

            _executionThread = new Thread(Run)
            {
                Name = "Zero Game"
            };
            _executionThread.Start();
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            Debug.LogInfo("Stopping server...");

            ZeroServer.Stop();
            await _stopped.Task
                .ConfigureAwait(false);

            await _serverSetup.StopDeploymentAsync()
                .ConfigureAwait(false);
            await _serverSetup.StopWorkerAsync()
                .ConfigureAwait(false);
        }

        private void Run()
        {
            try
            {
                ZeroServer.Run();
            }
            catch (Exception e)
            {
                Debug.LogCritical(e, "A critical error occurred");
            }
            finally
            {
                _stopped.TrySetResult(true);
            }
        }
    }
}
