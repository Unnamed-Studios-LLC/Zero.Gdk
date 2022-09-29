using Microsoft.AspNetCore.Mvc;
using Zero.Game.Local.Services.Abstract;
using Zero.Game.Server;

namespace Zero.Game.Local.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class ConnectionController : ControllerBase
    {
        private readonly IConnectionService _connectionService;

        public ConnectionController(IConnectionService connectionService)
        {
            _connectionService = connectionService;
        }

        [HttpPost]
        public IActionResult Start(StartConnectionRequest request)
        {
            var serviceResponse = _connectionService.Start(request);
            return StatusCode(serviceResponse.StatusCode, serviceResponse.Object);
        }
    }
}
