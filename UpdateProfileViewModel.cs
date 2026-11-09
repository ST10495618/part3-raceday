using System.ComponentModel.DataAnnotations;

namespace RaceDayMVC.Models.ViewModels
{
    public class UpdateProfileViewModel
    {

        [Required(ErrorMessage = "Please enter your first name.")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please enter your last name.")]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please enter your email address.")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
        public string Email { get; set; } = string.Empty;

        public string? Phone { get; set; }
    }
}
