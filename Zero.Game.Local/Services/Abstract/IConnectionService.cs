using UnnamedStudios.Common.Model;
using Zero.Game.Common;

namespace Zero.Game.Local.Services.Abstract
{
    public interface IConnectionService
    {
        ServiceResponse<StartConnectionResponse> Start(StartConnectionRequest request);
    }
}
