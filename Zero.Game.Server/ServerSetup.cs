using System.Threading.Tasks;
using Zero.Game.Common;

namespace Zero.Game.Server
{
    public abstract class ServerSetup
    {
        public abstract void BuildServerSchema(ServerSchemaBuilder builder);

        public abstract Task DeferredWorldResponseAsync(StartWorldResponse response);

        public abstract ServerOptions GetOptions();

        public abstract bool NewConnection(Connection connection);

        public abstract bool NewWorld(World world);

        public abstract Task StartDeploymentAsync();

        public abstract Task StartWorkerAsync();

        public abstract Task StopDeploymentAsync();

        public abstract Task StopWorkerAsync();
    }
}
