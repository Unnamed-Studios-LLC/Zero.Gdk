using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Logging.Console;
using Microsoft.Extensions.Options;
using System;
using System.IO;
using System.Threading.Tasks;
using Zero.Game.Local.Logging;
using Zero.Game.Local.Providers;
using Zero.Game.Local.Services.Hosted;
using Zero.Game.Server;
using Zero.Game.Shared;

namespace Zero.Game.Local
{
    public class ZeroLocal
    {
        internal static ZeroServer Server { get; set; }

        public static async Task RunAsync<TPlugin>(string[] args)
            where TPlugin : ServerPlugin, new()
        {
            var plugin = new TPlugin();
            var aspNetHost = CreateHostBuilder(args, plugin).Build();
            var zeroServerHost = CreateZeroGameBuilder(args, plugin).Build();

            var aspNetLifetime = aspNetHost.Services.GetService<IHostApplicationLifetime>();
            var zeroServerLifetime = zeroServerHost.Services.GetService<IHostApplicationLifetime>();
            aspNetLifetime.ApplicationStopping.Register(zeroServerLifetime.StopApplication);

            var aspNetTask = aspNetHost.RunAsync();
            var zeroServerTask = zeroServerHost.RunAsync();

            await Task.WhenAll(aspNetTask, zeroServerTask)
                .ConfigureAwait(false);
        }

        public static IHostBuilder CreateHostBuilder(string[] args, ServerPlugin plugin) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup(x => new Startup(x.Configuration, plugin));
                    webBuilder.UseUrls("https://localhost:4001", "http://localhost:4000");
                })
                .ConfigureLogging(logging => logging.ClearProviders());

        public static IHostBuilder CreateZeroGameBuilder(string[] args, ServerPlugin plugin) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices(services =>
                {
                    services.Configure<HostOptions>(opts => opts.ShutdownTimeout = TimeSpan.FromSeconds(20));

                    // add providers
                    services.AddTransient<IDeploymentProvider, LocalDeploymentProvider>();
                    services.AddTransient<ILoggingProvider, GameLogger>();
                    services.AddSingleton(plugin);

                    // add hosted
                    services.AddTransient<GameService>();

                    services.AddTransient<IHost, ZeroServerHost>();
                })
                .ConfigureLogging(logging =>
                {
                    logging.ClearProviders();
                    logging.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Trace);
                    logging.AddConsole(options => options.FormatterName = nameof(ZeroConsoleFormatter))
                        .AddConsoleFormatter<ZeroConsoleFormatter, ConsoleFormatterOptions>();
                });
    }
}
