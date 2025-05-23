using System.ComponentModel.DataAnnotations;

namespace AfriBase.ViewModels
{
    /// <summary>
    /// View model for the login page
    /// </summary>
    public class LoginViewModel
    {
        /// <summary>
        /// User's email address
        /// </summary>
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// User's password
        /// </summary>
        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; } = string.Empty;

        /// <summary>
        /// Flag for "Remember me" functionality
        /// </summary>
        [Display(Name = "Remember me")]
        public bool RememberMe { get; set; }

        /// <summary>
        /// URL to redirect to after successful login
        /// </summary>
        public string ReturnUrl { get; set; } = string.Empty;
    }
}