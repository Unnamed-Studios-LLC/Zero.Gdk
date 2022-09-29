using System.Threading.Tasks;
using Zero.Game.Shared;

namespace Zero.Game.Server
{
    public interface IDeploymentProvider
    {
        string GetHost();

        Task<StartConnectionResponse> StartConnectionAsync(StartConnectionRequest request);

        Task<StartWorldResponse> StartWorldAsync(StartWorldRequest request);

        Task StopWorldAsync(uint worldId);
    }
}
