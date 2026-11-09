namespace RaceDay.Models
{
    public class UserRole
    {
        public int UserRoleID { get; set; }

        public int UserID { get; set; }
        public int RoleID { get; set; }

        public User User { get; set; } = null!;
        public Role Role { get; set; } = null!;
    }
}
