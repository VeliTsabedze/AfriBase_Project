using AfriBase.Models;
using System.Collections.Generic;

namespace AfriBase.ViewModels
{
    /// <summary>
    /// View model for the bid dashboard
    /// </summary>
    public class BidDashboardViewModel
    {
        /// <summary>
        /// Collection of all active bids in the system
        /// </summary>
        public List<BidSummaryViewModel> ActiveBids { get; set; } = new List<BidSummaryViewModel>();

        /// <summary>
        /// Collection of all recent bids in the system
        /// </summary>
        public List<BidSummaryViewModel> RecentBids { get; set; } = new List<BidSummaryViewModel>();

        /// <summary>
        /// Collection of artifacts currently available for bidding
        /// </summary>
        public List<ArtifactSummaryViewModel> AvailableArtifacts { get; set; } = new List<ArtifactSummaryViewModel>();

        /// <summary>
        /// Statistics about bidding activity
        /// </summary>
        public BidStatisticsViewModel Statistics { get; set; } = new BidStatisticsViewModel();
    }

    /// <summary>
    /// View model for summarizing a bid
    /// </summary>
    public class BidSummaryViewModel
    {
        /// <summary>
        /// ID of the bid
        /// </summary>
        public int BidId { get; set; }

        /// <summary>
        /// ID of the artifact being bid on
        /// </summary>
        public int ArtifactId { get; set; }

        /// <summary>
        /// Name of the artifact
        /// </summary>
        public string ArtifactName { get; set; }

        /// <summary>
        /// Name of the user who placed the bid
        /// </summary>
        public string BidderName { get; set; }

        /// <summary>
        /// Amount of the bid
        /// </summary>
        public decimal BidAmount { get; set; }

        /// <summary>
        /// Date the bid was placed
        /// </summary>
        public string BidDate { get; set; }

        /// <summary>
        /// Current status of the bid
        /// </summary>
        public BidStatus Status { get; set; }

        /// <summary>
        /// URL to the image of the artifact
        /// </summary>
        public string ArtifactImageUrl { get; set; }
    }

    /// <summary>
    /// View model for summarizing an artifact
    /// </summary>
    public class ArtifactSummaryViewModel
    {
        /// <summary>
        /// ID of the artifact
        /// </summary>
        public int ArtifactId { get; set; }

        /// <summary>
        /// Name of the artifact
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Origin of the artifact
        /// </summary>
        public string Origin { get; set; }

        /// <summary>
        /// Estimated year of creation
        /// </summary>
        public int EstimatedYear { get; set; }

        /// <summary>
        /// Starting bid for the artifact
        /// </summary>
        public decimal StartingBid { get; set; }

        /// <summary>
        /// Current highest bid on the artifact
        /// </summary>
        public decimal CurrentHighestBid { get; set; }

        /// <summary>
        /// Number of bids placed on the artifact
        /// </summary>
        public int BidCount { get; set; }

        /// <summary>
        /// URL to the image of the artifact
        /// </summary>
        public string ImageUrl { get; set; }

        /// <summary>
        /// Category of the artifact
        /// </summary>
        public ArtifactCategory Category { get; set; }
    }

    /// <summary>
    /// View model for bid statistics
    /// </summary>
    public class BidStatisticsViewModel
    {
        /// <summary>
        /// Total number of bids in the system
        /// </summary>
        public int TotalBids { get; set; }

        /// <summary>
        /// Number of bids placed in the last 24 hours
        /// </summary>
        public int BidsLast24Hours { get; set; }

        /// <summary>
        /// Number of artifacts currently available for bidding
        /// </summary>
        public int AvailableArtifacts { get; set; }

        /// <summary>
        /// Total value of active bids
        /// </summary>
        public decimal TotalBidValue { get; set; }

        /// <summary>
        /// The artifact with the most bids
        /// </summary>
        public string MostBiddedArtifact { get; set; }

        /// <summary>
        /// The number of bids on the most bidded artifact
        /// </summary>
        public int MostBiddedArtifactCount { get; set; }
    }
}