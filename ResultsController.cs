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
    public class ResultsController : Controller
    {
        private readonly RaceDayContext _context;

        public ResultsController(RaceDayContext context)
        {
            _context = context;
        }

        // GET: api/results
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetResults()
        {
            var results = await _context.Results
                .Include(r => r.Registration)
                    .ThenInclude(r => r.Event)
                .AsNoTracking()
                .ToListAsync();

            return Ok(results);
        }

        // POST: api/results
        [HttpPost]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> AddResult(Result result)
        {
            var registration = await _context.Registrations
                .Include(r => r.Event)
                .FirstOrDefaultAsync(r =>
                    r.RegistrationID == result.RegistrationID);

            if (registration == null)
                return NotFound(new { message = "Registration not found." });

            var userIdValue = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (!int.TryParse(userIdValue, out int userId))
                return Unauthorized();

            if (registration.Event.OrganiserID != userId)
                return Forbid();

            bool resultExists = await _context.Results
                .AnyAsync(r =>
                    r.RegistrationID == result.RegistrationID);

            if (resultExists)
                return Conflict(new
                {
                    message = "A result already exists for this registration."
                });

            result.RecordedAt = DateTime.UtcNow;

            _context.Results.Add(result);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Result recorded successfully.",
                resultId = result.ResultID
            });
        }
    }
}
