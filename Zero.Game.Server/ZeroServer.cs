using System.Threading;
using System.Threading.Tasks;
using Zero.Game.Common;
using Zero.Game.Shared;

namespace Zero.Game.Server
{
    public static class ZeroServer
    {
        public static int Capacity => CapacityInternal;
        public static Node Node { get; private set; }

        internal static int CapacityInternal { get; set; }

        public static void Run()
        {
            GameSynchronizationContext.InitializeOnCurrentThread();
            Node.Start();
            Ticker.Run(DoTick);
            GameSynchronizationContext.Close(5_000);
        }

        public static void Setup(ServerSetup setup, IDeploymentProvider deploymentProvider, ILoggingProvider loggingProvider)
        {
            ServerDomain.DeploymentProvider = deploymentProvider;
            ServerDomain.LoggingProvider = loggingProvider;
            ServerDomain.Options = setup.GetOptions();
            ServerDomain.PrivateLogLevel = LogLevel.Trace;
            ServerDomain.Schema = BuildSchema(setup);
            ServerDomain.Setup = setup;

            ZeroCommon.Setup(loggingProvider, ServerDomain.Options, ServerDomain.PrivateLogLevel, ServerDomain.Schema);
            Node = ServerDomain.CreateNode();
        }

        public static void Stop()
        {
            Ticker.Stop();
            Node.Stop();
        }

        private static ServerSchema BuildSchema(ServerSetup setup)
        {
            var builder = new ServerSchemaBuilder();
            setup.BuildServerSchema(builder);
            return builder.Build();
        }

        private static void DoTick()
        {
            GameSynchronizationContext.Run();
            Node.Tick(Update.IsViewUpdate);
        }
    }
}
