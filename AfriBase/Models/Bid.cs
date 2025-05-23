using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AfriBase.Models
{
    /// <summary>
    /// Represents a bid placed on an artifact by a user
    /// </summary>
    public class Bid
    {
        /// <summary>
        /// Unique identifier for the bid
        /// </summary>
        [Key]
        public int BidId { get; set; }

        /// <summary>
        /// ID of the user who placed the bid
        /// </summary>
        [Required]
        public string UserId { get; set; }

        /// <summary>
        /// Navigation property to the user who placed the bid
        /// </summary>
        [ForeignKey("UserId")]
        public ApplicationUser User { get; set; }

        /// <summary>
        /// ID of the artifact being bid on
        /// </summary>
        [Required]
        public int ArtifactId { get; set; }

        /// <summary>
        /// Navigation property to the artifact being bid on
        /// </summary>
        [ForeignKey("ArtifactId")]
        public Artifact Artifact { get; set; }

        /// <summary>
        /// The amount of money bid by the user
        /// </summary>
        [Required(ErrorMessage = "Bid amount is required")]
        [Range(1, 999999999, ErrorMessage = "Bid amount must be greater than 0")]
        [DataType(DataType.Currency)]
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Bid Amount")]
        public decimal BidAmount { get; set; }

        /// <summary>
        /// The date and time when the bid was placed
        /// </summary>
        [DataType(DataType.DateTime)]
        [Display(Name = "Bid Date")]
        public DateTime BidDate { get; set; } = DateTime.Now;

        /// <summary>
        /// Current status of the bid
        /// </summary>
        [Required]
        [Display(Name = "Status")]
        public BidStatus Status { get; set; } = BidStatus.Pending;

        /// <summary>
        /// Optional comments provided by the bidder
        /// </summary>
        [StringLength(500, ErrorMessage = "Comments cannot exceed 500 characters")]
        [Display(Name = "Comments")]
        public string Comments { get; set; }

        /// <summary>
        /// Flag indicating if this bid has been logged to the external log file
        /// </summary>
        [Display(Name = "Is Logged")]
        public bool IsLogged { get; set; } = false;
    }

    /// <summary>
    /// Possible statuses for a bid
    /// </summary>
    public enum BidStatus
    {
        Pending,
        Accepted,
        Rejected,
        Outbid,
        Withdrawn,
        Won
    }
}