using System.ComponentModel.DataAnnotations;

namespace AfriBase.ViewModels
{
    /// <summary>
    /// View model for the registration page
    /// </summary>
    public class RegisterViewModel
    {
        /// <summary>
        /// User's first name
        /// </summary>
        [Required(ErrorMessage = "First name is required")]
        [StringLength(50, ErrorMessage = "First name cannot exceed 50 characters")]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        /// <summary>
        /// User's last name
        /// </summary>
        [Required(ErrorMessage = "Last name is required")]
        [StringLength(50, ErrorMessage = "Last name cannot exceed 50 characters")]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        /// <summary>
        /// User's email address (used as username)
        /// </summary>
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        [Display(Name = "Email")]
        public string Email { get; set; }

        /// <summary>
        /// User's phone number
        /// </summary>
        [Phone(ErrorMessage = "Invalid phone number")]
        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; }

        /// <summary>
        /// User's country of residence
        /// </summary>
        [Required(ErrorMessage = "Country is required")]
        [StringLength(100, ErrorMessage = "Country cannot exceed 100 characters")]
        [Display(Name = "Country")]
        public string Country { get; set; }

        /// <summary>
        /// User's password
        /// </summary>
        [Required(ErrorMessage = "Password is required")]
        [StringLength(100, ErrorMessage = "Password must be at least {2} characters long", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        /// <summary>
        /// Confirmation of the user's password
        /// </summary>
        [DataType(DataType.Password)]
        [Display(Name = "Confirm Password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match")]
        public string ConfirmPassword { get; set; }

        /// <summary>
        /// Optional bio/description for the user
        /// </summary>
        [StringLength(500, ErrorMessage = "Bio cannot exceed 500 characters")]
        [DataType(DataType.MultilineText)]
        [Display(Name = "Bio")]
        public string Bio { get; set; }
    }
}