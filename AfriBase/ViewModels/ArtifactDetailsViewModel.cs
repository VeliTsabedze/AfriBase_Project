using AfriBase.Models;
using System;
using System.Collections.Generic;

namespace AfriBase.ViewModels
{
    /// <summary>
    /// View model for displaying detailed information about an artifact
    /// </summary>
    public class ArtifactDetailsViewModel
    {
        /// <summary>
        /// The artifact being displayed
        /// </summary>
        public Artifact Artifact { get; set; }

        /// <summary>
        /// List of recent bids on this artifact
        /// </summary>
        public List<BidSummaryViewModel> RecentBids { get; set; } = new List<BidSummaryViewModel>();

        /// <summary>
        /// The current highest bid on this artifact
        /// </summary>
        public Bid HighestBid { get; set; }

        /// <summary>
        /// View model for placing a new bid
        /// </summary>
        public BidViewModel NewBidViewModel { get; set; }

        /// <summary>
        /// Information about the owner/seller of the artifact
        /// </summary>
        public ArtifactOwnerViewModel OwnerInfo { get; set; }

        /// <summary>
        /// List of similar artifacts that might interest the user
        /// </summary>
        public List<ArtifactSummaryViewModel> SimilarArtifacts { get; set; } = new List<ArtifactSummaryViewModel>();

        /// <summary>
        /// Number of people watching this artifact
        /// </summary>
        public int WatchCount { get; set; }

        /// <summary>
        /// Flag indicating if the current user is authorized to edit this artifact
        /// </summary>
        public bool CanEdit { get; set; }

        /// <summary>
        /// Flag indicating if the current user is authorized to bid on this artifact
        /// </summary>
        public bool CanBid { get; set; }

        /// <summary>
        /// The minimum acceptable bid amount
        /// </summary>
        public decimal MinimumBidAmount
        {
            get
            {
                if (HighestBid != null)
                {
                    return HighestBid.BidAmount + 1;
                }
                else if (Artifact.StartingBid.HasValue)
                {
                    return Artifact.StartingBid.Value;
                }
                else
                {
                    return 1;
                }
            }
        }

        /// <summary>
        /// Time remaining until bidding closes (if applicable)
        /// </summary>
        public TimeSpan? BidTimeRemaining { get; set; }
    }

    /// <summary>
    /// View model for displaying information about an artifact's owner/seller
    /// </summary>
    public class ArtifactOwnerViewModel
    {
        /// <summary>
        /// ID of the owner user
        /// </summary>
        public string UserId { get; set; }

        /// <summary>
        /// Full name of the owner
        /// </summary>
        public string FullName { get; set; }

        /// <summary>
        /// Country of the owner
        /// </summary>
        public string Country { get; set; }

        /// <summary>
        /// URL to the owner's profile image
        /// </summary>
        public string ProfileImageUrl { get; set; }

        /// <summary>
        /// Owner's account verification status
        /// </summary>
        public bool IsVerified { get; set; }

        /// <summary>
        /// Number of artifacts the owner has listed
        /// </summary>
        public int ArtifactCount { get; set; }

        /// <summary>
        /// Biography or description of the owner
        /// </summary>
        public string Bio { get; set; }

        /// <summary>
        /// Date the owner registered on the site
        /// </summary>
        public DateTime RegistrationDate { get; set; }
    }
}