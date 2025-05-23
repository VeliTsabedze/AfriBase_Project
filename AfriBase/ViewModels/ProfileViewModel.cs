using AfriBase.Models;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AfriBase.ViewModels
{
    /// <summary>
    /// View model for the user profile page
    /// </summary>
    public class ProfileViewModel
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
        /// User's email address
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
        /// User's profile bio/description
        /// </summary>
        [StringLength(500, ErrorMessage = "Bio cannot exceed 500 characters")]
        [DataType(DataType.MultilineText)]
        [Display(Name = "Bio")]
        public string Bio { get; set; }

        /// <summary>
        /// Current URL to the user's profile image
        /// </summary>
        [Display(Name = "Current Profile Image")]
        public string ProfileImageUrl { get; set; }

        /// <summary>
        /// File upload for the user's profile image
        /// </summary>
        [Display(Name = "Upload Profile Image")]
        public IFormFile ProfileImage { get; set; }

        /// <summary>
        /// Recent bids placed by the user
        /// </summary>
        public List<Bid> RecentBids { get; set; } = new List<Bid>();

        /// <summary>
        /// Artifacts owned by the user
        /// </summary>
        public List<Artifact> OwnedArtifacts { get; set; } = new List<Artifact>();
    }
}