using System;
using System.Threading.Tasks;
using Zero.Game.Common;
using Zero.Game.Shared;

namespace Zero.Game.Client
{
    public static class ZeroClient
    {
        public static async Task<(bool Success, ZeroClientInstance Client)> ConnectAsync(string ip, int port, string key)
        {
            var client = new ZeroClientInstance();

            try
            {
                if (!ClientDomain.Setup.NewClient(client))
                {
                    ClientDomain.InternalLog(LogLevel.Information, "{0} returned false", nameof(ClientDomain.Setup.NewClient));
                    return default;
                }
            }
            catch (Exception e)
            {
                ClientDomain.InternalLog(LogLevel.Error, e, "An error occurred during {0}", nameof(ClientDomain.Setup.NewClient));
                return default;
            }

            if (!await client.ConnectAsync(ip, port, key)
                .ConfigureAwait(false))
            {
                ClientDomain.InternalLog(LogLevel.Information, "Failed to connect to remote server");
                return default;
            }

            client.Start();
            return (true, client);
        }

        public static void Setup(ClientSetup setup, ILoggingProvider loggingProvider)
        {
            ClientDomain.LoggingProvider = loggingProvider;
            ClientDomain.Options = setup.GetOptions();
            ClientDomain.Schema = BuildSchema(setup);
            ClientDomain.Setup = setup;

            ZeroCommon.Setup(loggingProvider, ClientDomain.Options, LogLevel.Trace, ClientDomain.Schema);
        }

        private static ClientSchema BuildSchema(ClientSetup setup)
        {
            var builder = new ClientSchemaBuilder();
            setup.BuildClientSchema(builder);
            return builder.Build();
        }
    }
}
