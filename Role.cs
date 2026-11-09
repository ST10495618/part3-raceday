namespace RaceDay.Models
{
    public class Role
    {
        public int RoleID { get; set; }

        public string RoleName { get; set; } = string.Empty;

        public ICollection<UserRole> UserRoles { get; set; }
            = new List<UserRole>();
    }
}
