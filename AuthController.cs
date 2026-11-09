using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using RaceDay.Data;
using RaceDay.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using RaceDay.DTOs;

namespace RaceDay.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : Controller
    {
        private readonly RaceDayContext _context;
        private readonly IConfiguration _configuration;
        private readonly PasswordHasher<User> _passwordHasher = new();

        public AuthController(
            RaceDayContext context,
            IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<IActionResult> Register(
            [FromBody] RegisterRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            string email = request.Email.Trim().ToLowerInvariant();

            bool emailExists = await _context.Users
                .AnyAsync(u => u.Email == email);

            if (emailExists)
            {
                return BadRequest(new
                {
                    message = "This email address is already registered."
                });
            }

            string roleName = request.Role.Trim();

            if (roleName.Equals(
                "Participant",
                StringComparison.OrdinalIgnoreCase))
            {
                roleName = "Participant";
            }
            else if (roleName.Equals(
                "Organiser",
                StringComparison.OrdinalIgnoreCase))
            {
                roleName = "Organiser";
            }
            else
            {
                return BadRequest(new
                {
                    message = "Role must be Participant or Organiser."
                });
            }

            var role = await _context.Roles
                .FirstOrDefaultAsync(r => r.RoleName == roleName);

            if (role == null)
            {
                return StatusCode(500, new
                {
                    message = "The requested role was not found in the database."
                });
            }

            var user = new User
            {
                FirstName = request.FirstName.Trim(),
                LastName = request.LastName.Trim(),
                Email = email,
                Phone = request.Phone,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            user.PasswordHash = _passwordHasher.HashPassword(
                user,
                request.Password);

            // Save the user and their role in one transaction.
            await using var transaction =
                await _context.Database.BeginTransactionAsync();

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var userRole = new UserRole
            {
                UserID = user.UserID,
                RoleID = role.RoleID
            };

            _context.UserRoles.Add(userRole);
            await _context.SaveChangesAsync();

            await transaction.CommitAsync();

            return Ok(new
            {
                message = "Registration successful.",
                userId = user.UserID,
                email = user.Email,
                role = role.RoleName
            });
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login(
            [FromBody] LoginRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            string email = request.Email.Trim().ToLowerInvariant();

            var user = await _context.Users
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Email == email);


            if (user == null ||
                !user.IsActive ||
                string.IsNullOrWhiteSpace(user.PasswordHash))
            {
                return Unauthorized(new
                {
                    message = "Invalid email or password."
                });
            }


            var passwordResult =
                _passwordHasher.VerifyHashedPassword(
                    user,
                    user.PasswordHash,
                    request.Password);

            if (passwordResult ==
                PasswordVerificationResult.Failed)
            {
                return Unauthorized(new
                {
                    message = "Invalid email or password."
                });
            }

            var claims = new List<Claim>
            {
                new Claim(
                    ClaimTypes.NameIdentifier,
                    user.UserID.ToString()),

                new Claim(
                    ClaimTypes.Name,
                    user.FirstName),

                new Claim(
                    ClaimTypes.Email,
                    user.Email)
            };

            foreach (var userRole in user.UserRoles)
            {
                claims.Add(new Claim(
                    ClaimTypes.Role,
                    userRole.Role.RoleName));
            }

            string? key = _configuration["Jwt:Key"];
            string? issuer = _configuration["Jwt:Issuer"];
            string? audience = _configuration["Jwt:Audience"];

            if (string.IsNullOrWhiteSpace(key) ||
                string.IsNullOrWhiteSpace(issuer) ||
                string.IsNullOrWhiteSpace(audience))
            {
                return StatusCode(500, new
                {
                    message = "JWT configuration is missing."
                });
            }

            if (Encoding.UTF8.GetByteCount(key) < 32)
            {
                return StatusCode(500, new
                {
                    message = "The JWT signing key must be at least 32 bytes."
                });
            }

            var signingKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(key));

            var credentials = new SigningCredentials(
                signingKey,
                SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: credentials);

            string tokenString =
                new JwtSecurityTokenHandler().WriteToken(token);

            return Ok(new
            {
                message = "Login successful.",
                token = tokenString,
                expiresAt = token.ValidTo,
                user = new
                {
                    user.UserID,
                    user.FirstName,
                    user.LastName,
                    user.Email,
                    roles = user.UserRoles
                        .Select(ur => ur.Role.RoleName)
                        .ToList()
                }
            });
        }
    }
}
