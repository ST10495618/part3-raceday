using Microsoft.Extensions.Logging;

namespace RaceDay.Models
{
    public class User
    {
        public int UserID { get; set; }

        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string? Phone { get; set; }

        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; }

        public ICollection<UserRole> UserRoles { get; set; }
            = new List<UserRole>();

        public ICollection<Event> OrganisedEvents { get; set; }
            = new List<Event>();

        public ICollection<Registration> Registrations { get; set; }
            = new List<Registration>();
    }
}
