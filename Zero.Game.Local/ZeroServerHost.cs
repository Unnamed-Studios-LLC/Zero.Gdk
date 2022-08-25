using Microsoft.Extensions.Hosting;
using System;
using System.Threading;
using System.Threading.Tasks;
using Zero.Game.Local.Services.Hosted;

namespace Zero.Game.Local
{
    public class ZeroServerHost : IHost
    {
        private readonly GameService _gameService;

        public ZeroServerHost(IServiceProvider services,
            GameService gameService)
        {
            Services = services;
            _gameService = gameService;
        }

        public IServiceProvider Services { get; }

        public void Dispose()
        {

        }

        public async Task StartAsync(CancellationToken cancellationToken = default)
        {
            await _gameService.StartAsync(cancellationToken)
                .ConfigureAwait(false);
        }

        public async Task StopAsync(CancellationToken cancellationToken = default)
        {
            await _gameService.StopAsync(cancellationToken)
                .ConfigureAwait(false);
        }
    }
}
