using System.ComponentModel.DataAnnotations;

namespace BatteryShop.DataAccess.ViewModels
{
    public class SignupViewModel
    {
        [Required(ErrorMessage = "Owner name is required.")]
        [Display(Name = "Owner Name")]
        public string OwnerName { get; set; }

        [Required(ErrorMessage = "Phone number is required.")]
        [Display(Name = "Phone Number")]
        [Phone(ErrorMessage = "Invalid phone number.")]
        public string OwnerPhone { get; set; }

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        public string OwnerEmail { get; set; }

        [Required(ErrorMessage = "Password is required.")]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters.")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required(ErrorMessage = "Confirm password is required.")]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm Password")]
        [Compare("Password", ErrorMessage = "Passwords do not match.")]
        public string ConfirmPassword { get; set; }
    }
}
