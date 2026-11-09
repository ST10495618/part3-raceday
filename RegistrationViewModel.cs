namespace RaceDayMVC.Models.ViewModels
{
    public class RegistrationViewModel
    {
        public int RegistrationID { get; set; }

        public int UserID { get; set; }

        public int EventID { get; set; }

        public DateTime RegistrationDate { get; set; }

        public string Status { get; set; } = string.Empty;

        public string? RegistrationDetails { get; set; }

        public ParticipantDetailsViewModel? ParticipantDetails { get; set; }

        public EventViewModel? EventDetails { get; set; }
    }
}
