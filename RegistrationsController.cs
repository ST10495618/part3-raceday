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
    [Authorize]
    public class RegistrationsController : Controller
    {
        private readonly RaceDayContext _context;

        public RegistrationsController(RaceDayContext context)
        {
            _context = context;
        }

        // GET: api/registrations
        [HttpGet]
        public async Task<IActionResult> GetRegistrations()
        {
            var query = _context.Registrations
                .AsNoTracking()
                .AsQueryable();

            // Participants can only see their own registrations.
            if (!User.IsInRole("Organiser"))
            {
                var userIdValue = User.FindFirstValue(
                    ClaimTypes.NameIdentifier);

                if (!int.TryParse(userIdValue, out int userId))
                    return Unauthorized();

                query = query.Where(r => r.UserID == userId);
            }

            var registrations = await query
                .Select(r => new
                {
                    registrationID = r.RegistrationID,
                    userID = r.UserID,
                    eventID = r.EventID,
                    registrationDate = r.RegistrationDate,
                    status = r.Status,
                    registrationDetails = r.RegistrationDetails,

                    eventDetails = r.Event == null ? null : new
                    {
                        eventID = r.Event.EventID,
                        name = r.Event.Name,
                        description = r.Event.Description,
                        eventDate = r.Event.EventDate,
                        eventTime = r.Event.EventTime,
                        location = r.Event.Location,
                        entryFee = r.Event.EntryFee,
                        maxParticipants = r.Event.MaxParticipants,
                        status = r.Event.Status
                    }
                })
                .ToListAsync();

            return Ok(registrations);
        }

        // POST: api/registrations
        [HttpPost]
        [Authorize(Roles = "Participant")]
        public async Task<IActionResult> RegisterForEvent(
            Registration registration)
        {
            var userIdValue = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (!int.TryParse(userIdValue, out int userId))
                return Unauthorized();

            var raceEvent = await _context.Events
                .FirstOrDefaultAsync(e => e.EventID == registration.EventID);

            if (raceEvent == null)
                return NotFound(new { message = "Event not found." });

            if (raceEvent.EventDate.Date < DateTime.UtcNow.Date)
                return BadRequest(new { message = "This event has already passed." });

            bool alreadyRegistered = await _context.Registrations
                .AnyAsync(r => r.UserID == userId
                    && r.EventID == registration.EventID);

            if (alreadyRegistered)
                return Conflict(new { message = "You are already registered for this event." });

            int registrationCount = await _context.Registrations
                .CountAsync(r => r.EventID == registration.EventID);

            if (registrationCount >= raceEvent.MaxParticipants)
                return Conflict(new { message = "This event is full." });

            registration.UserID = userId;
            registration.RegistrationDate = DateTime.UtcNow;
            registration.Status = "Registered";

            _context.Registrations.Add(registration);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Registration successful.",
                registrationId = registration.RegistrationID,
                eventId = registration.EventID
            });
        }

        // DELETE: api/registrations/1
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Participant")]
        public async Task<IActionResult> CancelRegistration(int id)
        {
            var userIdValue = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (!int.TryParse(userIdValue, out int userId))
                return Unauthorized();

            var registration = await _context.Registrations
                .FirstOrDefaultAsync(r =>
                    r.RegistrationID == id && r.UserID == userId);

            if (registration == null)
                return NotFound(new
                {
                    message = "Registration not found for this participant."
                });

            registration.Status = "Cancelled";
            await _context.SaveChangesAsync();

            return Ok(new { message = "Registration cancelled successfully." });
        }
    }
}
