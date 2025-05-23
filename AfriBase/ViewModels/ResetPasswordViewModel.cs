using System.ComponentModel.DataAnnotations;

namespace AfriBase.ViewModels
{
    /// <summary>
    /// View model for the reset password page
    /// </summary>
    public class ResetPasswordViewModel
    {
        /// <summary>
        /// User's email address
        /// </summary>
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        [Display(Name = "Email")]
        public string Email { get; set; }

        /// <summary>
        /// New password
        /// </summary>
        [Required(ErrorMessage = "Password is required")]
        [StringLength(100, ErrorMessage = "Password must be at least {2} characters long", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        /// <summary>
        /// Confirmation of the new password
        /// </summary>
        [DataType(DataType.Password)]
        [Display(Name = "Confirm Password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match")]
        public string ConfirmPassword { get; set; }

        /// <summary>
        /// Password reset code
        /// </summary>
        public string Code { get; set; }
    }
}