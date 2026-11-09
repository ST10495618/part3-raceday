namespace RaceDay.Models
{
    public class Registration
    {
        public int RegistrationID { get; set; }

        public int UserID { get; set; }
        public int EventID { get; set; }

        public DateTime RegistrationDate { get; set; }
        public string Status { get; set; } = "Registered";
        public string? RegistrationDetails { get; set; }

        // Navigation properties
        public User? User { get; set; }
        public Event? Event { get; set; }

        public Result? Result { get; set; }
    }
}
