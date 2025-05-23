using System.ComponentModel.DataAnnotations;

namespace AfriBase.ViewModels
{
    /// <summary>
    /// View model for submitting a bid on an artifact
    /// </summary>
    public class BidViewModel
    {
        /// <summary>
        /// ID of the artifact being bid on
        /// </summary>
        [Required]
        public int ArtifactId { get; set; }

        /// <summary>
        /// Name of the artifact (for display only)
        /// </summary>
        [Display(Name = "Artifact")]
        public string ArtifactName { get; set; }

        /// <summary>
        /// Current highest bid on the artifact
        /// </summary>
        [Display(Name = "Current Highest Bid")]
        [DataType(DataType.Currency)]
        public decimal CurrentHighestBid { get; set; }

        /// <summary>
        /// Starting bid for the artifact
        /// </summary>
        [Display(Name = "Starting Bid")]
        [DataType(DataType.Currency)]
        public decimal StartingBid { get; set; }

        /// <summary>
        /// Amount being bid by the user
        /// </summary>
        [Required(ErrorMessage = "Bid amount is required")]
        [Range(0.01, 9999999, ErrorMessage = "Bid amount must be greater than 0")]
        [DataType(DataType.Currency)]
        [Display(Name = "Your Bid")]
        public decimal BidAmount { get; set; }

        /// <summary>
        /// Optional comments from the bidder
        /// </summary>
        [StringLength(500, ErrorMessage = "Comments cannot exceed 500 characters")]
        [Display(Name = "Comments")]
        public string Comments { get; set; }

        /// <summary>
        /// URL to the image of the artifact
        /// </summary>
        public string ArtifactImageUrl { get; set; }

        /// <summary>
        /// Validation method to ensure bid amount is higher than current highest bid
        /// </summary>
        /// <returns>True if valid, false otherwise</returns>
        public bool ValidateBidAmount()
        {
            if (CurrentHighestBid > 0)
            {
                return BidAmount > CurrentHighestBid;
            }

            return BidAmount >= StartingBid;
        }
    }
}