using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace AfriBase.Models
{
    /// <summary>
    /// Represents an African artifact in the system
    /// </summary>
    public class Artifact
    {
        /// <summary>
        /// Unique identifier for the artifact
        /// </summary>
        [Key]
        public int ArtifactId { get; set; }

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
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Estimated Value")]
        public decimal EstimatedValue { get; set; }

        /// <summary>
        /// Category of the artifact (e.g., Mask, Sculpture, Textile)
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
        /// URL to the primary image of the artifact
        /// </summary>
        [Display(Name = "Image")]
        public string ImageUrl { get; set; }

        /// <summary>
        /// Date and time when the artifact was added to the system
        /// </summary>
        [DataType(DataType.DateTime)]
        [Display(Name = "Date Added")]
        public DateTime DateAdded { get; set; } = DateTime.Now;

        /// <summary>
        /// Optional starting price for auction bidding
        /// </summary>
        [DataType(DataType.Currency)]
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Starting Bid")]
        public decimal? StartingBid { get; set; }

        /// <summary>
        /// Flag indicating if the artifact is currently available for bidding
        /// </summary>
        [Display(Name = "Available for Bidding")]
        public bool IsAvailableForBidding { get; set; }

        /// <summary>
        /// ID of the user who owns/submitted this artifact
        /// </summary>
        [Display(Name = "Owner")]
        public string OwnerId { get; set; }

        /// <summary>
        /// Navigation property to the ApplicationUser who owns the artifact
        /// </summary>
        [ForeignKey("OwnerId")]
        public ApplicationUser Owner { get; set; }

        /// <summary>
        /// Navigation property to all bids placed on this artifact
        /// </summary>
        public virtual ICollection<Bid> Bids { get; set; } = new List<Bid>();

        /// <summary>
        /// Gets the highest current bid on this artifact, if any
        /// </summary>
        [NotMapped]
        public Bid HighestBid
        {
            get
            {
                return Bids.OrderByDescending(b => b.BidAmount).FirstOrDefault();
            }
        }

        /// <summary>
        /// Regional area in Africa where the artifact originated
        /// </summary>
        [Display(Name = "Region")]
        public AfricanRegion Region { get; set; }

        // Removed the problematic property:
        // public object AddedDate { get; internal set; }
    }

    /// <summary>
    /// Categories of African artifacts
    /// </summary>
    public enum ArtifactCategory
    {
        Mask,
        Sculpture,
        Textile,
        Jewelry,
        Pottery,
        Weapon,
        MusicalInstrument,
        Furniture,
        ReligiousItem,
        Painting,
        Other
    }

    /// <summary>
    /// Possible physical conditions of artifacts
    /// </summary>
    public enum ArtifactCondition
    {
        Excellent,
        VeryGood,
        Good,
        Fair,
        Poor,
        Damaged,
        Restored
    }

    /// <summary>
    /// Geographic regions of Africa
    /// </summary>
    public enum AfricanRegion
    {
        NorthAfrica,
        WestAfrica,
        CentralAfrica,
        EastAfrica,
        SouthernAfrica,
        HornOfAfrica
    }
}