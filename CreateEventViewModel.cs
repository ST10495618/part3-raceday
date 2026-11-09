using System.ComponentModel.DataAnnotations;

namespace RaceDayMVC.Models.ViewModels
{
    public class CreateEventViewModel
    {
        [Required(ErrorMessage = "Event name is required.")]
        [Display(Name = "Event Name")]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "Description")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Please select an event date.")]
        [DataType(DataType.Date)]
        [Display(Name = "Event Date")]
        public DateTime EventDate { get; set; } = DateTime.Today;

        [Required(ErrorMessage = "Event time is required.")]
        [RegularExpression(@"^([01]\d|2[0-3]):[0-5]\d$",
     ErrorMessage = "Enter a valid time.")]
        [Display(Name = "Event Time")]
        public string EventTime { get; set; } = string.Empty;

        [Required(ErrorMessage = "Event location is required.")]
        public string Location { get; set; } = string.Empty;

        [Range(0, 100000, ErrorMessage = "Entry fee cannot be negative.")]
        [Display(Name = "Entry Fee (R)")]
        public decimal EntryFee { get; set; }

        [Required]
        [Range(1, 100000, ErrorMessage = "Maximum participants must be at least 1.")]
        [Display(Name = "Maximum Participants")]
        public int MaxParticipants { get; set; }

        [Required(ErrorMessage = "Please select a category.")]
        [Display(Name = "Category ID")]
        [Range(1, int.MaxValue, ErrorMessage = "Please select a valid category.")]
        public int CategoryID { get; set; }

        [Required]
        public string Status { get; set; } = "Open";
        [Display(Name = "Event Banner Image")]
        public IFormFile? BannerImage { get; set; }
    }
}
