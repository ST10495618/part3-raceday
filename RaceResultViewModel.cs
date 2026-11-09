namespace RaceDayMVC.Models.ViewModels
{
    public class RaceResultViewModel
    {
        public int ResultID { get; set; }

        public int RegistrationID { get; set; }

        public int? Position { get; set; }

        public int? StartPosition { get; set; }

        public string? FinishTime { get; set; }

        public decimal? Pace { get; set; }

        public string Status { get; set; } = string.Empty;

        public DateTime RecordedAt { get; set; }

        public RegistrationViewModel? Registration { get; set; }

        public string ParticipantName { get; set; } = string.Empty;

        public string EventName { get; set; } = string.Empty;
        public int EventID { get; set; }
    }
}
