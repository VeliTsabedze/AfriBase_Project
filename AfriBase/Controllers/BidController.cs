using AfriBase.Data;
using AfriBase.Models;
using AfriBase.Services;
using AfriBase.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace AfriBase.Controllers
{
    /// <summary>
    /// Controller handling bidding operations
    /// </summary>
    public class BidController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        IBidLogService bidLogService,
        ILogger<BidController> logger) : Controller
    {
        private readonly ApplicationDbContext _context = context;
        private readonly UserManager<ApplicationUser> _userManager = userManager;
        private readonly IBidLogService _bidLogService = bidLogService;
        private readonly ILogger<BidController> _logger = logger;

        /// <summary>
        /// Displays a list of the user's bids
        /// </summary>
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Index()
        {
            try
            {
                // Gets the current user
                var userId = await GetCurrentUserId();
                if (userId == null)
                {
                    return RedirectToAction("Login", "Account");
                }

                // Gets the user's bids with related data
                var userBids = await _context.Bids
                    .Where(b => b.UserId == userId)
                    .Include(b => b.Artifact)
                    .OrderByDescending(b => b.BidDate)
                    .ToListAsync();

                return View(userBids);
            }
            catch (Exception ex)
            {
                // Logs the exception and redirect to an error page
                _logger.LogError(ex, "Error retrieving user bids");
                return RedirectToAction("Error", "Home");
            }
        }

        /// <summary>
        /// Process a new bid submission
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Bidder")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PlaceBid(BidViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToAction("Details", "Artifact", new { id = model.ArtifactId });
            }

            try
            {
                // Get the current user
                var userId = await GetCurrentUserId();
                if (userId == null)
                {
                    return RedirectToAction("Login", "Account");
                }

                // Get the artifact
                var artifact = await _context.Artifacts
                    .Include(a => a.Bids)
                    .FirstOrDefaultAsync(a => a.ArtifactId == model.ArtifactId);

                if (artifact == null)
                {
                    return NotFound();
                }

                // Check if the artifact is available for bidding
                if (!artifact.IsAvailableForBidding)
                {
                    TempData["ErrorMessage"] = "This artifact is not available for bidding.";
                    return RedirectToAction("Details", "Artifact", new { id = model.ArtifactId });
                }

                // Check if the user is the owner of the artifact
                if (artifact.OwnerId == userId)
                {
                    TempData["ErrorMessage"] = "You cannot bid on your own artifact.";
                    return RedirectToAction("Details", "Artifact", new { id = model.ArtifactId });
                }

                // Get the current highest bid
                var highestBid = artifact.Bids.OrderByDescending(b => b.BidAmount).FirstOrDefault();

                // Check if the bid amount is valid
                if (highestBid != null && model.BidAmount <= highestBid.BidAmount)
                {
                    TempData["ErrorMessage"] = $"Your bid must be higher than the current highest bid (${highestBid.BidAmount}).";
                    return RedirectToAction("Details", "Artifact", new { id = model.ArtifactId });
                }
                else if (artifact.StartingBid.HasValue && model.BidAmount < artifact.StartingBid.Value)
                {
                    TempData["ErrorMessage"] = $"Your bid must be at least the starting bid amount (${artifact.StartingBid.Value}).";
                    return RedirectToAction("Details", "Artifact", new { id = model.ArtifactId });
                }

                // Update previous highest bid to "Outbid" status
                if (highestBid != null)
                {
                    highestBid.Status = BidStatus.Outbid;
                    _context.Update(highestBid);
                }

                // Create a new bid
                var bid = new Bid
                {
                    UserId = userId,
                    ArtifactId = model.ArtifactId,
                    BidAmount = model.BidAmount,
                    BidDate = DateTime.Now,
                    Status = BidStatus.Pending,
                    Comments = model.Comments
                };

                // Add the bid to the database
                _context.Add(bid);
                await _context.SaveChangesAsync();

                // Log the bid
                await _bidLogService.LogBidAsync(bid, artifact.Name);

                // Set success message
                TempData["SuccessMessage"] = "Your bid has been placed successfully.";

                return RedirectToAction("Details", "Artifact", new { id = model.ArtifactId });
            }
            catch (Exception ex)
            {
                // Log the exception and display a generic error message
                _logger.LogError(ex, "Error placing bid for artifact {ArtifactId}", model.ArtifactId);
                TempData["ErrorMessage"] = "An error occurred while processing your bid. Please try again later.";
                return RedirectToAction("Details", "Artifact", new { id = model.ArtifactId });
            }
        }

        /// <summary>
        /// Display the dashboard for admins to monitor bidding activity
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Dashboard()
        {
            try
            {
                // Create the dashboard view model
                var model = new BidDashboardViewModel();

                // Get active bids (pending bids)
                var activeBids = await _context.Bids
                    .Where(b => b.Status == BidStatus.Pending)
                    .Include(b => b.Artifact)
                    .Include(b => b.User)
                    .OrderByDescending(b => b.BidDate)
                    .Take(10)
                    .ToListAsync();

                // Map active bids to view model
                model.ActiveBids = activeBids.Select(b => new BidSummaryViewModel
                {
                    BidId = b.BidId,
                    ArtifactId = b.ArtifactId,
                    ArtifactName = b.Artifact.Name,
                    BidderName = b.User.FullName,
                    BidAmount = b.BidAmount,
                    BidDate = b.BidDate.ToString("MMM dd, yyyy HH:mm"),
                    Status = b.Status,
                    ArtifactImageUrl = b.Artifact.ImageUrl
                }).ToList();

                // Get recent bids (all statuses)
                var recentBids = await _context.Bids
                    .Include(b => b.Artifact)
                    .Include(b => b.User)
                    .OrderByDescending(b => b.BidDate)
                    .Take(20)
                    .ToListAsync();

                // Map recent bids to view model
                model.RecentBids = recentBids.Select(b => new BidSummaryViewModel
                {
                    BidId = b.BidId,
                    ArtifactId = b.ArtifactId,
                    ArtifactName = b.Artifact.Name,
                    BidderName = b.User.FullName,
                    BidAmount = b.BidAmount,
                    BidDate = b.BidDate.ToString("MMM dd, yyyy HH:mm"),
                    Status = b.Status,
                    ArtifactImageUrl = b.Artifact.ImageUrl
                }).ToList();

                // Get available artifacts
                var availableArtifacts = await _context.Artifacts
                    .Where(a => a.IsAvailableForBidding)
                    .Include(a => a.Bids)
                    .OrderByDescending(a => a.DateAdded)
                    .Take(10)
                    .ToListAsync();

                // Calculate highest bid amount for each artifact to avoid null propagation issues
                model.AvailableArtifacts = availableArtifacts.Select(a =>
                {
                    decimal highestBidAmount = 0;
                    if (a.Bids.Any())
                    {
                        highestBidAmount = a.Bids.Max(b => b.BidAmount);
                    }

                    return new ArtifactSummaryViewModel
                    {
                        ArtifactId = a.ArtifactId,
                        Name = a.Name,
                        Origin = a.Origin,
                        EstimatedYear = a.EstimatedYear,
                        StartingBid = a.StartingBid ?? 0,
                        CurrentHighestBid = highestBidAmount,
                        BidCount = a.Bids.Count,
                        ImageUrl = a.ImageUrl,
                        Category = a.Category
                    };
                }).ToList();

                // Calculate statistics
                model.Statistics = new BidStatisticsViewModel
                {
                    TotalBids = await _context.Bids.CountAsync(),
                    BidsLast24Hours = await _context.Bids.CountAsync(b => b.BidDate >= DateTime.Now.AddDays(-1)),
                    AvailableArtifacts = await _context.Artifacts.CountAsync(a => a.IsAvailableForBidding),
                    TotalBidValue = await _context.Bids.Where(b => b.Status == BidStatus.Pending).SumAsync(b => b.BidAmount)
                };

                // Get the most bidded artifact
                var mostBiddedArtifact = await _context.Artifacts
                    .Include(a => a.Bids)
                    .OrderByDescending(a => a.Bids.Count)
                    .FirstOrDefaultAsync();

                if (mostBiddedArtifact != null)
                {
                    model.Statistics.MostBiddedArtifact = mostBiddedArtifact.Name;
                    model.Statistics.MostBiddedArtifactCount = mostBiddedArtifact.Bids.Count;
                }

                return View(model);
            }
            catch (Exception ex)
            {
                // Log the exception and redirect to an error page
                _logger.LogError(ex, "Error loading bid dashboard");
                return RedirectToAction("Error", "Home");
            }
        }

        /// <summary>
        /// Display bids for a specific artifact
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "Admin,Seller")]
        public async Task<IActionResult> ArtifactBids(int id)
        {
            try
            {
                // Get the artifact with its bids
                var artifact = await _context.Artifacts
                    .Include(a => a.Bids)
                        .ThenInclude(b => b.User)
                    .FirstOrDefaultAsync(a => a.ArtifactId == id);

                if (artifact == null)
                {
                    return NotFound();
                }

                // Check if the current user is authorized to view these bids
                var userId = await GetCurrentUserId();
                if (userId != artifact.OwnerId && !User.IsInRole("Admin"))
                {
                    return RedirectToAction("AccessDenied", "Account");
                }

                ViewData["ArtifactName"] = artifact.Name;
                ViewData["ArtifactId"] = artifact.ArtifactId;

                // Order bids by date (most recent first)
                var bids = artifact.Bids.OrderByDescending(b => b.BidDate).ToList();

                return View(bids);
            }
            catch (Exception ex)
            {
                // Log the exception and redirect to an error page
                _logger.LogError(ex, "Error retrieving bids for artifact {ArtifactId}", id);
                return RedirectToAction("Error", "Home");
            }
        }

        /// <summary>
        /// Display bid history for the current user
        /// </summary>
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> UserBidHistory()
        {
            try
            {
                // Get the current user
                var userId = await GetCurrentUserId();
                if (userId == null)
                {
                    return RedirectToAction("Login", "Account");
                }

                // Get all bids placed by the user
                var userBids = await _context.Bids
                    .Where(b => b.UserId == userId)
                    .Include(b => b.Artifact)
                    .OrderByDescending(b => b.BidDate)
                    .ToListAsync();

                return View(userBids);
            }
            catch (Exception ex)
            {
                // Log the exception and redirect to an error page
                _logger.LogError(ex, "Error retrieving bid history for user");
                return RedirectToAction("Error", "Home");
            }
        }

        /// <summary>
        /// Process cancellation of a user's bid
        /// </summary>
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelBid(int id)
        {
            try
            {
                // Get the bid
                var bid = await _context.Bids.FindAsync(id);
                if (bid == null)
                {
                    return NotFound();
                }

                // Check if the current user is authorized to cancel this bid
                var userId = await GetCurrentUserId();
                if (userId != bid.UserId)
                {
                    return RedirectToAction("AccessDenied", "Account");
                }

                // Check if the bid can be canceled (only pending bids can be canceled)
                if (bid.Status != BidStatus.Pending)
                {
                    TempData["ErrorMessage"] = "This bid cannot be canceled because its status is not pending.";
                    return RedirectToAction(nameof(UserBidHistory));
                }

                // Update the bid status
                bid.Status = BidStatus.Withdrawn;
                _context.Update(bid);
                await _context.SaveChangesAsync();

                // Log the bid cancellation
                await _bidLogService.LogBidCancellationAsync(bid);

                TempData["SuccessMessage"] = "Your bid has been canceled successfully.";
                return RedirectToAction(nameof(UserBidHistory));
            }
            catch (Exception ex)
            {
                // Log the exception and display a generic error message
                _logger.LogError(ex, "Error canceling bid {BidId}", id);
                TempData["ErrorMessage"] = "An error occurred while processing your request. Please try again later.";
                return RedirectToAction(nameof(UserBidHistory));
            }
        }

        /// <summary>
        /// Process acceptance of a bid by an admin or artifact owner
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Admin,Seller")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AcceptBid(int id)
        {
            try
            {
                // Get the bid with related data
                var bid = await _context.Bids
                    .Include(b => b.Artifact)
                    .FirstOrDefaultAsync(b => b.BidId == id);

                if (bid == null)
                {
                    return NotFound();
                }

                // Check if the current user is authorized to accept this bid
                var userId = await GetCurrentUserId();
                if (userId != bid.Artifact.OwnerId && !User.IsInRole("Admin"))
                {
                    return RedirectToAction("AccessDenied", "Account");
                }

                // Update all bids for this artifact to reject them except the accepted one
                var otherBids = await _context.Bids
                    .Where(b => b.ArtifactId == bid.ArtifactId && b.BidId != bid.BidId)
                    .ToListAsync();

                foreach (var otherBid in otherBids)
                {
                    otherBid.Status = BidStatus.Rejected;
                    _context.Update(otherBid);
                }

                // Updates the accepted bid
                bid.Status = BidStatus.Won;
                _context.Update(bid);

                // Updates the artifact to no longer be available for bidding
                bid.Artifact.IsAvailableForBidding = false;
                _context.Update(bid.Artifact);

                await _context.SaveChangesAsync();

                // Logs the bid acceptance
                await _bidLogService.LogBidAcceptanceAsync(bid);

                TempData["SuccessMessage"] = "The bid has been accepted successfully.";
                return RedirectToAction("ArtifactBids", new { id = bid.ArtifactId });
            }
            catch (Exception ex)
            {
                // Logs the exception and display a generic error message
                _logger.LogError(ex, "Error accepting bid {BidId}", id);
                TempData["ErrorMessage"] = "An error occurred while processing your request. Please try again later.";
                return RedirectToAction("Dashboard");
            }
        }

        /// <summary>
        /// Process rejection of a bid by an admin or artifact owner
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Admin,Seller")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectBid(int id)
        {
            try
            {
                // Gets the bid with related data
                var bid = await _context.Bids
                    .Include(b => b.Artifact)
                    .FirstOrDefaultAsync(b => b.BidId == id);

                if (bid == null)
                {
                    return NotFound();
                }

                // Checks if the current user is authorized to reject this bid
                var userId = await GetCurrentUserId();
                if (userId != bid.Artifact.OwnerId && !User.IsInRole("Admin"))
                {
                    return RedirectToAction("AccessDenied", "Account");
                }

                // Updates the bid
                bid.Status = BidStatus.Rejected;
                _context.Update(bid);
                await _context.SaveChangesAsync();

                // Logs the bid rejection
                await _bidLogService.LogBidRejectionAsync(bid);

                TempData["SuccessMessage"] = "The bid has been rejected successfully.";
                return RedirectToAction("ArtifactBids", new { id = bid.ArtifactId });
            }
            catch (Exception ex)
            {
                // Logs the exception and display a generic error message
                _logger.LogError(ex, "Error rejecting bid {BidId}", id);
                TempData["ErrorMessage"] = "An error occurred while processing your request. Please try again later.";
                return RedirectToAction("Dashboard");
            }
        }

        /// <summary>
        /// Helper method to get the current user's ID
        /// </summary>
        private async Task<string?> GetCurrentUserId()
        {
            var user = await _userManager.GetUserAsync(User);
            return user?.Id;
        }
    }
}