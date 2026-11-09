using System.ComponentModel.DataAnnotations;

namespace RaceDayMVC.Models.ViewModels
{
    public class RecordResultViewModel
    {

        [Required]
        public int RegistrationID { get; set; }

        [Range(1, int.MaxValue)]
        public int? Position { get; set; }

        [Range(1, int.MaxValue)]
        public int? StartPosition { get; set; }

        public string? FinishTime { get; set; }

        [Range(typeof(decimal), "0", "1000")]
        public decimal? Pace { get; set; }

        public string Status { get; set; } = "Completed";

        public List<RegistrationViewModel> Registrations { get; set; }
            = new List<RegistrationViewModel>();
    }
}
