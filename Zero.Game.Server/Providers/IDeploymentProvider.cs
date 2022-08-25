using System.Threading.Tasks;
using Zero.Game.Common;

namespace Zero.Game.Server
{
    public interface IDeploymentProvider
    {
        Task<StartConnectionResponse> StartConnectionAsync(StartConnectionRequest request);

        Task<StartWorldResponse> StartWorldAsync(StartWorldRequest request);

        Task StopWorldAsync(uint worldId);
    }
}
