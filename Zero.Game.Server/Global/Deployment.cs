using System.Collections.Generic;
using System.Threading.Tasks;
using Zero.Game.Model;

namespace Zero.Game.Server
{
    public static class Deployment
    {
        public static string HostIp => ServerDomain.DeploymentProvider.GetHost();

        public static void GetAllWorlds(List<WorldInfo> outputList) => ServerDomain.DeploymentProvider.GetAllWorldInfos(outputList);
        public static Task<StartConnectionResponse> StartConnectionAsync(StartConnectionRequest request) => ServerDomain.DeploymentProvider.StartConnectionAsync(request);
        public static Task<StartWorldResponse> StartWorldAsync(StartWorldRequest request) => ServerDomain.DeploymentProvider.StartWorldAsync(request);
        public static Task StopWorldAsync(uint worldId) => ServerDomain.DeploymentProvider.StopWorldAsync(worldId);
        public static bool TryGetWorld(uint worldId, out WorldInfo worldInfo) => ServerDomain.DeploymentProvider.TryGet(worldId, out worldInfo);
    }
}
