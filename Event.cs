namespace RaceDay.Models
{
    public class Event
    {
        public int EventID { get; set; }
        public int OrganiserID { get; set; }
        public int CategoryID { get; set; }

        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }

        public DateTime EventDate { get; set; }
        public TimeSpan EventTime { get; set; }

        public string Location { get; set; } = string.Empty;
        public decimal EntryFee { get; set; }
        public int MaxParticipants { get; set; }
        public string Status { get; set; } = "Open";
        public DateTime CreatedAt { get; set; }

        public User Organiser { get; set; } = null!;
        public Category Category { get; set; } = null!;

        public ICollection<Registration> Registrations { get; set; }
            = new List<Registration>();

        public Route? Route { get; set; }

        public ICollection<Weather> WeatherForecasts { get; set; }
            = new List<Weather>();
    }
}
