using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AfriBase.Models
{
    /// <summary>
    /// Extends the ASP.NET Core Identity user with additional properties for AfriBase
    /// </summary>
    public class ApplicationUser : IdentityUser
    {
        /// <summary>
        /// User's first name
        /// </summary>
        [PersonalData]
        [StringLength(50, ErrorMessage = "First name cannot exceed 50 characters")]
        [Display(Name = "First Name")]
        public string FirstName { get; set; } = string.Empty;

        /// <summary>
        /// User's last name
        /// </summary>
        [PersonalData]
        [StringLength(50, ErrorMessage = "Last name cannot exceed 50 characters")]
        [Display(Name = "Last Name")]
        public string LastName { get; set; } = string.Empty;

        /// <summary>
        /// User's date of registration
        /// </summary>
        [DataType(DataType.DateTime)]
        [Display(Name = "Registration Date")]
        public DateTime RegistrationDate { get; set; } = DateTime.Now;

        /// <summary>
        /// User's profile bio/description
        /// </summary>
        [StringLength(500, ErrorMessage = "Bio cannot exceed 500 characters")]
        [DataType(DataType.MultilineText)]
        [Display(Name = "Bio")]
        public string Bio { get; set; } = string.Empty;

        /// <summary>
        /// URL to the user's profile image
        /// </summary>
        [Display(Name = "Profile Image")]
        public string ProfileImageUrl { get; set; } = "/images/default-profile.jpg";

        /// <summary>
        /// User's country of residence
        /// </summary>
        [StringLength(100, ErrorMessage = "Country cannot exceed 100 characters")]
        [Display(Name = "Country")]
        public string Country { get; set; } = string.Empty;

        /// <summary>
        /// Flag indicating if the user has verified their account
        /// </summary>
        [Display(Name = "Is Verified")]
        public bool IsVerified { get; set; } = false;

        /// <summary>
        /// Date of the user's last login
        /// </summary>
        [DataType(DataType.DateTime)]
        [Display(Name = "Last Login")]
        public DateTime? LastLoginDate { get; set; }

        /// <summary>
        /// Navigation property to artifacts owned by this user
        /// </summary>
        public virtual ICollection<Artifact> OwnedArtifacts { get; set; } = new List<Artifact>();

        /// <summary>
        /// Navigation property to bids placed by this user
        /// </summary>
        public virtual ICollection<Bid> Bids { get; set; } = new List<Bid>();

        /// <summary>
        /// Returns the user's full name
        /// </summary>
        public string FullName => $"{FirstName} {LastName}";

        /// <summary>
        /// Calculates if the user is considered active (logged in within the last 30 days)
        /// </summary>
        public bool IsActive => LastLoginDate.HasValue && LastLoginDate.Value > DateTime.Now.AddDays(-30);
    }
}