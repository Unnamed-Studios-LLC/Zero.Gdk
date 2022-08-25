using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using Zero.Game.Local.Providers;
using Zero.Game.Local.Services.Hosted;
using Zero.Game.Server;
using Zero.Game.Shared;

namespace Zero.Game.Local
{
    public class ZeroLocal
    {
        public static async Task RunAsync<TSetup>(string[] args)
            where TSetup : ServerSetup, new()
        {
            var setup = new TSetup();
            var aspNetHost = CreateHostBuilder(args, setup).Build();
            var zeroServerHost = CreateZeroGameBuilder(args, setup).Build();

            var aspNetLifetime = aspNetHost.Services.GetService<IHostApplicationLifetime>();
            var zeroServerLifetime = zeroServerHost.Services.GetService<IHostApplicationLifetime>();
            aspNetLifetime.ApplicationStopping.Register(zeroServerLifetime.StopApplication);

            var aspNetTask = aspNetHost.RunAsync();
            var zeroServerTask = zeroServerHost.RunAsync();

            await Task.WhenAll(aspNetTask, zeroServerTask)
                .ConfigureAwait(false);
        }

        public static IHostBuilder CreateHostBuilder(string[] args, ServerSetup setup) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup(x => new Startup(x.Configuration, setup));
                })
                .ConfigureLogging(logging =>
                {
                    logging.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Trace);
                    logging.AddFilter("Microsoft.*", x => false);
                });

        public static IHostBuilder CreateZeroGameBuilder(string[] args, ServerSetup setup) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices(services =>
                {
                    services.Configure<HostOptions>(opts => opts.ShutdownTimeout = TimeSpan.FromSeconds(15));

                    // add providers
                    services.AddTransient<IDeploymentProvider, LocalDeploymentProvider>();
                    services.AddTransient<ILoggingProvider, GameLogger>();
                    services.AddSingleton(setup);

                    // add hosted
                    services.AddTransient<GameService>();

                    services.AddTransient<IHost, ZeroServerHost>();
                })
                .ConfigureLogging(logging =>
                {
                    logging.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Trace);
                    logging.AddFilter("Microsoft.*", x => false);
                });
    }
}
