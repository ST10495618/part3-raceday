using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RaceDay.Data;
using RaceDay.Models;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;

namespace RaceDay.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WeatherController : Controller
    {
        private readonly RaceDayContext _context;

        public WeatherController(RaceDayContext context)
        {
            _context = context;
        }

        // GET: api/weather
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetWeather()
        {
            var forecasts = await _context.WeatherForecasts
                .AsNoTracking()
                .ToListAsync();

            return Ok(forecasts);
        }

        // GET: api/weather/event/1
        [HttpGet("event/{eventId:int}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetWeatherForEvent(int eventId)
        {
            var forecasts = await _context.WeatherForecasts
                .Where(w => w.EventID == eventId)
                .AsNoTracking()
                .ToListAsync();

            return Ok(forecasts);
        }

        // POST: api/weather
        [HttpPost]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> AddWeather(Weather weather)
        {
            var raceEvent = await _context.Events
                .FirstOrDefaultAsync(e => e.EventID == weather.EventID);

            if (raceEvent == null)
                return NotFound(new { message = "Event not found." });

            var userIdValue = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (!int.TryParse(userIdValue, out int userId))
                return Unauthorized();

            if (raceEvent.OrganiserID != userId)
                return Forbid();

            if (weather.Humidity is < 0 or > 100)
                return BadRequest(new
                {
                    message = "Humidity must be between 0 and 100."
                });

            if (weather.RainProbability is < 0 or > 100)
                return BadRequest(new
                {
                    message = "Rain probability must be between 0 and 100."
                });

            weather.UpdatedAt = DateTime.UtcNow;

            _context.WeatherForecasts.Add(weather);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Weather forecast added successfully.",
                weatherId = weather.WeatherID
            });
        }
    }
}
