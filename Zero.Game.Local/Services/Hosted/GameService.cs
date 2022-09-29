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
        private readonly CancellationTokenSource _cancellationTokenSource = new();

        private readonly ILogger<GameService> _logger;
        private readonly IHostApplicationLifetime _lifetime;
        private readonly ServerPlugin _serverPlugin;
        private readonly IDeploymentProvider _deploymentProvider;
        private readonly ILoggingProvider _loggingProvider;

        public GameService(ILogger<GameService> logger,
            IHostApplicationLifetime lifetime,
            ServerPlugin serverPlugin,
            IDeploymentProvider deploymentProvider,
            ILoggingProvider loggingProvider)
        {
            _logger = logger;
            _lifetime = lifetime;
            _serverPlugin = serverPlugin;
            _deploymentProvider = deploymentProvider;
            _loggingProvider = loggingProvider;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            if (_serverPlugin == null)
            {
                _logger.LogError("Failed to start deployment. Unable to load valid build");
                Environment.ExitCode = 1;
                _lifetime.StopApplication();
                return;
            }

            ZeroLocal.Server = ZeroServer.Create(_loggingProvider, _deploymentProvider, _serverPlugin);

            Debug.LogInfo("Starting server...");
            Debug.LogInfo("Press Ctrl+C to stop");

            await _serverPlugin.StartWorkerAsync()
                .ConfigureAwait(false);
            await _serverPlugin.StartDeploymentAsync()
                .ConfigureAwait(false);

            _executionThread = new Thread(Run)
            {
                Name = "Zero Game"
            };
            _executionThread.Start();
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            Debug.LogInfo("Stopping server...");

            _cancellationTokenSource.Cancel();

            await _stopped.Task
                .ConfigureAwait(false);

            await _serverPlugin.StopDeploymentAsync()
                .ConfigureAwait(false);
            await _serverPlugin.StopWorkerAsync()
                .ConfigureAwait(false);

            _cancellationTokenSource.Dispose();
        }

        private void Run()
        {
            var server = ZeroLocal.Server;
            try
            {
                server.Run(_cancellationTokenSource.Token);
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
