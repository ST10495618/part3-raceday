using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RaceDay.Data;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using RaceDay.Models;

namespace RaceDay.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RoutesController : Controller
    {
        private readonly RaceDayContext _context;

        public RoutesController(RaceDayContext context)
        {
            _context = context;
        }

        // GET: api/routes
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetRoutes()
        {
            var routes = await _context.Routes
                .AsNoTracking()
                .ToListAsync();

            return Ok(routes);
        }

        // GET: api/routes/1
        [HttpGet("{id:int}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetRoute(int id)
        {
            var route = await _context.Routes
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.RouteID == id);

            if (route == null)
                return NotFound(new { message = "Route not found." });

            return Ok(route);
        }

        // POST: api/routes
        [HttpPost]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> CreateRoute(RaceDay.Models.Route route)
        {
            var raceEvent = await _context.Events
                .FirstOrDefaultAsync(e => e.EventID == route.EventID);

            if (raceEvent == null)
                return NotFound(new { message = "Event not found." });

            var userIdValue = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (!int.TryParse(userIdValue, out int userId))
                return Unauthorized();

            if (raceEvent.OrganiserID != userId)
                return Forbid();

            if (route.DistanceKm <= 0)
                return BadRequest(new
                {
                    message = "Route distance must be greater than zero."
                });

            _context.Routes.Add(route);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Route created successfully.",
                routeId = route.RouteID
            });
        }
    }
}
