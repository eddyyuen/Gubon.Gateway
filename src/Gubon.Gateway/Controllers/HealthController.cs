using Gubon.Gateway.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Gubon.Gateway.Controllers
{
    [ApiController]
    [Route("__admin")]
    public class HealthController : ControllerBase
    {
        /// <summary>
     /// Returns 200 if Proxy is healthy.
     /// </summary>
        [HttpGet]
        [Route("ping")]
        public IActionResult Ping()
        {
            return Ok("pong");
        }
    }
}
