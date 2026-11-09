namespace RaceDayMVC.Models.ViewModels
{
    public class ResultViewModel
    {
        public int ResultID { get; set; }

        public int RegistrationID { get; set; }

        public int UserID { get; set; }

        public int EventID { get; set; }

        public DateTime? EventDate { get; set; }

        public string EventName { get; set; } = string.Empty;

        public string ParticipantName { get; set; } = string.Empty;

        public int? Position { get; set; }

        public int? StartPosition { get; set; }

        public TimeSpan? FinishTime { get; set; }

        public decimal? Pace { get; set; }

        public string Status { get; set; } = string.Empty;

        public DateTime RecordedAt { get; set; }
    }

    public class ResultRegistrationViewModel
    {
        public int RegistrationID { get; set; }

        public int UserID { get; set; }

        public int EventID { get; set; }

        public DateTime RegistrationDate { get; set; }

        public string Status { get; set; } = string.Empty;

        public EventViewModel? Event { get; set; }
    }
}

