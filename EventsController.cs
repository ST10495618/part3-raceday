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
    public class EventsController : Controller
    {
        private readonly RaceDayContext _context;

        public EventsController(RaceDayContext context)
        {
            _context = context;
        }

        // GET: api/events
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetEvents()
        {
            var events = await _context.Events
                .Include(e => e.Category)
                .AsNoTracking()
                .ToListAsync();

            return Ok(events);
        }

        // GET: api/events/1
        [HttpGet("{id:int}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetEvent(int id)
        {
            var raceEvent = await _context.Events
                .Include(e => e.Category)
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.EventID == id);

            if (raceEvent == null)
                return NotFound(new { message = "Event not found." });

            return Ok(raceEvent);
        }

        // POST: api/events
        [HttpPost]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> CreateEvent(Event raceEvent)
        {
            if (raceEvent.EventDate.Date < DateTime.UtcNow.Date)
            {
                return BadRequest(new
                {
                    message = "The event date cannot be in the past."
                });
            }

            bool categoryExists = await _context.Categories
                .AnyAsync(c => c.CategoryID == raceEvent.CategoryID);

            if (!categoryExists)
                return BadRequest(new { message = "Invalid category." });

            var userIdValue = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (!int.TryParse(userIdValue, out int userId))
                return Unauthorized();

            raceEvent.OrganiserID = userId;
            raceEvent.CreatedAt = DateTime.UtcNow;

            _context.Events.Add(raceEvent);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetEvent),
                new { id = raceEvent.EventID },
                raceEvent);
        }

        // PUT: api/events/1
        [HttpPut("{id:int}")]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> UpdateEvent(
            int id, Event updatedEvent)
        {
            if (id != updatedEvent.EventID)
            {
                return BadRequest(new
                {
                    message = "The event ID does not match."
                });
            }

            var existingEvent = await _context.Events
                .FirstOrDefaultAsync(e => e.EventID == id);

            if (existingEvent == null)
                return NotFound(new { message = "Event not found." });

            var userIdValue = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (!int.TryParse(userIdValue, out int userId))
                return Unauthorized();

            if (existingEvent.OrganiserID != userId)
                return Forbid();

            bool categoryExists = await _context.Categories
                .AnyAsync(c => c.CategoryID == updatedEvent.CategoryID);

            if (!categoryExists)
                return BadRequest(new { message = "Invalid category." });

            existingEvent.CategoryID = updatedEvent.CategoryID;
            existingEvent.Name = updatedEvent.Name;
            existingEvent.Description = updatedEvent.Description;
            existingEvent.EventDate = updatedEvent.EventDate;
            existingEvent.EventTime = updatedEvent.EventTime;
            existingEvent.Location = updatedEvent.Location;
            existingEvent.EntryFee = updatedEvent.EntryFee;
            existingEvent.MaxParticipants = updatedEvent.MaxParticipants;
            existingEvent.Status = updatedEvent.Status;

            await _context.SaveChangesAsync();

            return Ok(existingEvent);
        }

        // DELETE: api/events/1
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> DeleteEvent(int id)
        {
            var raceEvent = await _context.Events
                .FirstOrDefaultAsync(e => e.EventID == id);

            if (raceEvent == null)
                return NotFound(new { message = "Event not found." });

            var userIdValue = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (!int.TryParse(userIdValue, out int userId))
                return Unauthorized();

            if (raceEvent.OrganiserID != userId)
                return Forbid();

            _context.Events.Remove(raceEvent);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Event deleted successfully." });
        }
    }
}
