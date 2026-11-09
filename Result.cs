namespace RaceDay.Models
{
    public class Result
    {
        public int ResultID { get; set; }
        public int RegistrationID { get; set; }

        public int? Position { get; set; }
        public int? StartPosition { get; set; }
        public TimeSpan? FinishTime { get; set; }
        public decimal? Pace { get; set; }

        public string Status { get; set; } = "Pending";
        public DateTime RecordedAt { get; set; }

        public Registration? Registration { get; set; }
    }
}
