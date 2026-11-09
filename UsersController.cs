using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RaceDay.Data;
using RaceDay.DTOs;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
namespace RaceDay.Controllers
{
    [ApiController]
    [Route("api/users")]
    [Authorize]
    public class UsersController : Controller
    {
        private readonly RaceDayContext _context;

        public UsersController(RaceDayContext context)
        {
            _context = context;
        }

        private bool TryGetUserId(out int userId)
        {
            return int.TryParse(
                User.FindFirstValue(ClaimTypes.NameIdentifier),
                out userId);
        }

        // GET: api/users/me
        [HttpGet("me")]
        public async Task<IActionResult> GetMyProfile()
        {
            if (!TryGetUserId(out int userId))
                return Unauthorized();

            var user = await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.UserID == userId);

            if (user == null)
                return NotFound(new { message = "User not found." });

            return Ok(new
            {
                user.UserID,
                user.FirstName,
                user.LastName,
                user.Email,
                user.Phone,
                user.IsActive,
                user.CreatedAt
            });
        }

        // PUT: api/users/me
        [HttpPut("me")]
        public async Task<IActionResult> UpdateMyProfile(
            UpdateProfileRequest request)
        {
            if (!TryGetUserId(out int userId))
                return Unauthorized();

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.UserID == userId);

            if (user == null)
                return NotFound(new { message = "User not found." });

            string email = request.Email.Trim().ToLowerInvariant();

            bool emailInUse = await _context.Users.AnyAsync(u =>
                u.Email.ToLower() == email &&
                u.UserID != userId);

            if (emailInUse)
            {
                return Conflict(new
                {
                    message = "That email address is already in use."
                });
            }

            user.FirstName = request.FirstName.Trim();
            user.LastName = request.LastName.Trim();
            user.Email = email;
            user.Phone = request.Phone;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Profile updated successfully.",
                user.UserID,
                user.FirstName,
                user.LastName,
                user.Email,
                user.Phone
            });
        }

        // DELETE: api/users/me
        [HttpDelete("me")]
        public async Task<IActionResult> DeactivateMyAccount()
        {
            if (!TryGetUserId(out int userId))
                return Unauthorized();

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.UserID == userId);

            if (user == null)
                return NotFound(new { message = "User not found." });

            user.IsActive = false;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Your account has been deactivated."
            });
        }
    }
}
