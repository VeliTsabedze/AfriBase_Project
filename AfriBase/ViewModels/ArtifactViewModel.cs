using AfriBase.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.ComponentModel.DataAnnotations;

namespace AfriBase.ViewModels
{
    /// <summary>
    /// View model for creating or editing an artifact
    /// </summary>
    public class ArtifactViewModel
    {
        /// <summary>
        /// ID of the artifact (null for new artifacts)
        /// </summary>
        public int? ArtifactId { get; set; }

        /// <summary>
        /// Name of the artifact
        /// </summary>
        [Required(ErrorMessage = "Artifact name is required")]
        [StringLength(200, ErrorMessage = "Name cannot exceed 200 characters")]
        [Display(Name = "Artifact Name")]
        public string Name { get; set; }

        /// <summary>
        /// Detailed description of the artifact
        /// </summary>
        [Required(ErrorMessage = "Description is required")]
        [DataType(DataType.MultilineText)]
        [Display(Name = "Description")]
        public string Description { get; set; }

        /// <summary>
        /// Geographical or cultural origin of the artifact
        /// </summary>
        [Required(ErrorMessage = "Origin is required")]
        [StringLength(100, ErrorMessage = "Origin cannot exceed 100 characters")]
        [Display(Name = "Origin")]
        public string Origin { get; set; }

        /// <summary>
        /// Estimated year of creation
        /// </summary>
        [Required(ErrorMessage = "Estimated year is required")]
        [Range(100, 2025, ErrorMessage = "Year must be between 100 and present year")]
        [Display(Name = "Estimated Year")]
        public int EstimatedYear { get; set; }

        /// <summary>
        /// Current estimated value of the artifact
        /// </summary>
        [Required(ErrorMessage = "Estimated value is required")]
        [DataType(DataType.Currency)]
        [Display(Name = "Estimated Value")]
        public decimal EstimatedValue { get; set; }

        /// <summary>
        /// Category of the artifact
        /// </summary>
        [Required(ErrorMessage = "Category is required")]
        [Display(Name = "Category")]
        public ArtifactCategory Category { get; set; }

        /// <summary>
        /// Current physical condition of the artifact
        /// </summary>
        [Required(ErrorMessage = "Condition is required")]
        [Display(Name = "Condition")]
        public ArtifactCondition Condition { get; set; }

        /// <summary>
        /// Current image URL of the artifact (if it exists)
        /// </summary>
        [Display(Name = "Current Image")]
        public string CurrentImageUrl { get; set; }

        /// <summary>
        /// File upload for the artifact image
        /// </summary>
        [Display(Name = "Upload Image")]
        public IFormFile ImageFile { get; set; }

        /// <summary>
        /// Optional starting price for auction bidding
        /// </summary>
        [DataType(DataType.Currency)]
        [Display(Name = "Starting Bid")]
        public decimal? StartingBid { get; set; }

        /// <summary>
        /// Flag indicating if the artifact is currently available for bidding
        /// </summary>
        [Display(Name = "Available for Bidding")]
        public bool IsAvailableForBidding { get; set; }

        /// <summary>
        /// Regional area in Africa where the artifact originated
        /// </summary>
        [Required(ErrorMessage = "Region is required")]
        [Display(Name = "Region")]
        public AfricanRegion Region { get; set; }

        /// <summary>
        /// Flag indicating if this is a new artifact being created
        /// </summary>
        public bool IsNew => !ArtifactId.HasValue;
    }
}