using System.Collections.Generic;
using System.Threading.Tasks;
using Zero.Game.Model;

namespace Zero.Game.Server
{
    public interface IDeploymentProvider
    {
        void GetAllWorldInfos(List<WorldInfo> outputList);
        string GetHost();
        void ReportConnectionCount(uint worldId, int count);
        Task<StartConnectionResponse> StartConnectionAsync(StartConnectionRequest request);
        Task<StartWorldResponse> StartWorldAsync(StartWorldRequest request);
        Task StopWorldAsync(uint worldId);
        bool TryGet(uint worldId, out WorldInfo worldInfo);
    }
}
