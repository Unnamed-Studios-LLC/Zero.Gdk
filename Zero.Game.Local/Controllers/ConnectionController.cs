using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Zero.Game.Local.Services;
using Zero.Game.Model;

namespace Zero.Game.Local.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class ConnectionController : ControllerBase
    {
        private readonly ConnectionService _connectionService;

        public ConnectionController(ConnectionService connectionService)
        {
            _connectionService = connectionService;
        }

        [HttpPost]
        public async Task<IActionResult> Start(StartConnectionRequest request)
        {
            var serviceResponse = await _connectionService.StartAsync(request);
            return StatusCode(serviceResponse.StatusCode, serviceResponse.Object);
        }
    }
}
