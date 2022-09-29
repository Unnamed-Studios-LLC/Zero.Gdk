using UnnamedStudios.Common.Model;
using Zero.Game.Shared;
using Zero.Game.Server;

namespace Zero.Game.Local.Services.Abstract
{
    public interface IConnectionService
    {
        ServiceResponse<StartConnectionResponse> Start(StartConnectionRequest request);
    }
}
