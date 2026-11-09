using Microsoft.AspNetCore.Mvc;
using RaceDay.Data;

namespace RaceDay.Controllers
{
    [ApiController]
    [Route("api/health")]
    public class HealthController : Controller
    {
        private readonly RaceDayContext _context;

        public HealthController(RaceDayContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetHealth()
        {
            try
            {
                bool databaseConnected =
                    await _context.Database.CanConnectAsync();

                if (!databaseConnected)
                {
                    return StatusCode(503, new
                    {
                        status = "Unhealthy",
                        database = "Disconnected"
                    });
                }

                return Ok(new
                {
                    status = "Healthy",
                    database = "Connected"
                });
            }
            catch
            {
                return StatusCode(503, new
                {
                    status = "Unhealthy",
                    database = "Unavailable"
                });
            }
        }
    }
}
