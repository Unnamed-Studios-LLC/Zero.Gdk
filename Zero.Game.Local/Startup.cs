using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using System;
using Zero.Game.Local.Providers;
using Zero.Game.Local.Services;
using Zero.Game.Local.Services.Abstract;
using Zero.Game.Server;
using Zero.Game.Shared;

namespace Zero.Game.Local
{
    internal class Startup
    {
        public Startup(IConfiguration configuration, ServerSetup setup)
        {
            Configuration = configuration;
            Setup = setup;
        }

        public IConfiguration Configuration { get; }

        public ServerSetup Setup { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<HostOptions>(opts => opts.ShutdownTimeout = TimeSpan.FromSeconds(15));

            // add providers
            services.AddTransient<IDeploymentProvider, LocalDeploymentProvider>();
            services.AddTransient<ILoggingProvider, GameLogger>();
            //services.AddSingleton(Setup);

            // add services
            services.AddScoped<IConnectionService, ConnectionService>();

            // add hosted
            //services.AddTransient<GameService>();

            services.AddRouting(options => options.LowercaseUrls = true);
            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Zero.Game.Local", Version = "v1" });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Zero.Game.Local v1"));
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
