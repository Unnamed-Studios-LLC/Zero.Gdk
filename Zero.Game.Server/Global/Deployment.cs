using System.Threading.Tasks;
using Zero.Game.Common;

namespace Zero.Game.Server
{
    public static class Deployment
    {
        public static Task<StartConnectionResponse> StartConnectionAsync(StartConnectionRequest request)
        {
            return ServerDomain.DeploymentProvider.StartConnectionAsync(request);
        }

        public static Task<StartWorldResponse> StartWorldAsync(StartWorldRequest request)
        {
            return ServerDomain.DeploymentProvider.StartWorldAsync(request);
        }

        public static Task StopWorldAsync(uint worldId)
        {
            return ServerDomain.DeploymentProvider.StopWorldAsync(worldId);
        }
    }
}
