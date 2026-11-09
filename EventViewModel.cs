namespace RaceDayMVC.Models.ViewModels
{
    public class EventViewModel
    {
        public int EventID { get; set; }

        public int OrganiserID { get; set; }

        public int CategoryID { get; set; }

        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        public DateTime EventDate { get; set; }

        public string EventTime { get; set; } = string.Empty;

        public string Location { get; set; } = string.Empty;

        public decimal EntryFee { get; set; }

        public int MaxParticipants { get; set; }

        public string Status { get; set; } = string.Empty;
        public string EventType { get; set; } = string.Empty;
        // Azure Blob Storage event banner
        public string? BannerImageUrl { get; set; }
    }
}
