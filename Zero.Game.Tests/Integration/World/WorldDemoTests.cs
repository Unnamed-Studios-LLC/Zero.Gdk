using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Zero.Game.Common;
using Zero.Game.Server;
using Zero.Game.Shared;

namespace Zero.Game.Tests.Integration.World
{
    public class WorldDemoTests
    {
        #region Objects

        private class TestComponent : Component, IAsyncComponent
        {
            public Task OnDestroyAsync()
            {
                return Task.CompletedTask;
            }

            public Task<bool> OnInitAsync()
            {
                var entity = new Entity();
                World.AddEntity(entity);

                var component = new TestComponent();
                entity.AddComponent(component);

                return Task.FromResult(true);
            }
        }

        #endregion

        #region Providers

        private class DeploymentProvider : IDeploymentProvider
        {
            public Task<StartConnectionResponse> StartConnectionAsync(StartConnectionRequest request)
            {
                throw new NotImplementedException();
            }

            public Task<StartWorldResponse> StartWorldAsync(StartWorldRequest request)
            {
                throw new NotImplementedException();
            }

            public Task StopWorldAsync(uint worldId)
            {
                throw new NotImplementedException();
            }
        }

        private class LoggingProvider : ILoggingProvider
        {
            private readonly object _lock = new object();

            public void Log(LogLevel logLevel, string message, Exception e)
            {
                lock (_lock)
                {
                    Console.WriteLine($"{logLevel.ToString().ToUpper()} | {message}");
                    if (e != null)
                    {
                        Console.WriteLine(e);
                    }
                }
            }

            public void Log(LogLevel logLevel, string format, object[] args, Exception e)
            {
                lock (_lock)
                {
                    Console.WriteLine($"{logLevel.ToString().ToUpper()} | {format}", args);
                    if (e != null)
                    {
                        Console.WriteLine(e);
                    }
                }
            }
        }

        #endregion

        private class Setup : ServerSetup
        {
            public override void BuildServerSchema(ServerSchemaBuilder builder)
            {
                builder.Component<TestComponent>();
            }

            public override Task DeferredWorldResponseAsync(StartWorldResponse response)
            {
                return Task.CompletedTask;
            }

            public override ServerOptions GetOptions()
            {
                return new ServerOptions
                {
                    UpdatesPerViewUpdate = 1,
                    TickIntervalMs = 50,
                    InternalLogLevel = LogLevel.Trace,
                    LogLevel = LogLevel.Trace,
                    Networking = new NetworkingOptions
                    {
                        MaxBufferSize = 10_000,
                        Mode = NetworkMode.Reliable,
                        Port = 24_000
                    }
                };
            }

            public override bool NewConnection(Connection connection)
            {
                return true;
            }

            public override bool NewWorld(Server.World world)
            {
                return true;
            }

            public override Task StartDeploymentAsync()
            {
                throw new NotImplementedException();
            }

            public override Task StartWorkerAsync()
            {
                throw new NotImplementedException();
            }

            public override Task StopDeploymentAsync()
            {
                throw new NotImplementedException();
            }

            public override Task StopWorkerAsync()
            {
                throw new NotImplementedException();
            }
        }

        [Test]
        public async Task Test()
        {
            ZeroServer.Setup(new Setup(), new DeploymentProvider(), new LoggingProvider());

            var worldTask = Task.Run(() =>
            {
                ZeroServer.Run();
            });

            await Task.Delay(500);

            var req = new StartWorldRequest
            {
                WorldId = 1,
                Data = new Dictionary<string, string>(),
            };

            await ZeroServer.Node.AddWorldAsync(req);

            await Task.Delay(1000);

            ZeroServer.Stop();
            await worldTask;
        }
    }
}
