using AfriBase.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using System;
using System.IO;
using System.Threading.Tasks;

namespace AfriBase.Services
{
    /// <summary>
    /// Service for logging bid operations to a text file
    /// </summary>
    public class BidLogService : IBidLogService
    {
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly string _logFilePath;

        /// <summary>
        /// Constructor with dependency injection
        /// </summary>
        public BidLogService(
            IWebHostEnvironment webHostEnvironment,
            UserManager<ApplicationUser> userManager)
        {
            _webHostEnvironment = webHostEnvironment;
            _userManager = userManager;

            // Set up log file path
            var logsDirectory = Path.Combine(_webHostEnvironment.ContentRootPath, "Logs");
            if (!Directory.Exists(logsDirectory))
            {
                Directory.CreateDirectory(logsDirectory);
            }

            _logFilePath = Path.Combine(logsDirectory, "bid_log.txt");
        }

        /// <summary>
        /// Log a new bid
        /// </summary>
        public async Task LogBidAsync(Bid bid, string artifactName)
        {
            try
            {
                // Get the user who placed the bid
                var user = await _userManager.FindByIdAsync(bid.UserId);
                var userName = user != null ? user.FullName : "Unknown User";

                // Create log entry
                var logEntry = $"[{DateTime.Now}] NEW BID: User '{userName}' placed a bid of ${bid.BidAmount} on artifact '{artifactName}' (ID: {bid.ArtifactId}). Bid ID: {bid.BidId}";

                // Write to log file
                await WriteToLogAsync(logEntry);

                // Mark the bid as logged
                bid.IsLogged = true;
            }
            catch (Exception ex)
            {
                // Log the error
                await WriteToLogAsync($"[{DateTime.Now}] ERROR: Failed to log bid: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Log the acceptance of a bid
        /// </summary>
        public async Task LogBidAcceptanceAsync(Bid bid)
        {
            try
            {
                // Get the user who placed the bid
                var user = await _userManager.FindByIdAsync(bid.UserId);
                var userName = user != null ? user.FullName : "Unknown User";

                // Create log entry
                var logEntry = $"[{DateTime.Now}] BID ACCEPTED: Bid ID {bid.BidId} by user '{userName}' for ${bid.BidAmount} on artifact '{bid.Artifact.Name}' (ID: {bid.ArtifactId}) was accepted.";

                // Write to log file
                await WriteToLogAsync(logEntry);
            }
            catch (Exception ex)
            {
                // Log the error
                await WriteToLogAsync($"[{DateTime.Now}] ERROR: Failed to log bid acceptance: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Log the rejection of a bid
        /// </summary>
        public async Task LogBidRejectionAsync(Bid bid)
        {
            try
            {
                // Get the user who placed the bid
                var user = await _userManager.FindByIdAsync(bid.UserId);
                var userName = user != null ? user.FullName : "Unknown User";

                // Create log entry
                var logEntry = $"[{DateTime.Now}] BID REJECTED: Bid ID {bid.BidId} by user '{userName}' for ${bid.BidAmount} on artifact '{bid.Artifact.Name}' (ID: {bid.ArtifactId}) was rejected.";

                // Write to log file
                await WriteToLogAsync(logEntry);
            }
            catch (Exception ex)
            {
                // Log the error
                await WriteToLogAsync($"[{DateTime.Now}] ERROR: Failed to log bid rejection: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Log the cancellation of a bid
        /// </summary>
        public async Task LogBidCancellationAsync(Bid bid)
        {
            try
            {
                // Get the user who placed the bid
                var user = await _userManager.FindByIdAsync(bid.UserId);
                var userName = user != null ? user.FullName : "Unknown User";

                // Create log entry
                var logEntry = $"[{DateTime.Now}] BID CANCELLED: Bid ID {bid.BidId} by user '{userName}' for ${bid.BidAmount} on artifact ID {bid.ArtifactId} was cancelled by the user.";

                // Write to log file
                await WriteToLogAsync(logEntry);
            }
            catch (Exception ex)
            {
                // Log the error
                await WriteToLogAsync($"[{DateTime.Now}] ERROR: Failed to log bid cancellation: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Write an entry to the log file
        /// </summary>
        private async Task WriteToLogAsync(string logEntry)
        {
            try
            {
                // Append to log file with a timestamp
                using (var writer = new StreamWriter(_logFilePath, true))
                {
                    await writer.WriteLineAsync(logEntry);
                }
            }
            catch (Exception ex)
            {
                // If we can't write to the log file, we can't do much more than throw
                throw new ApplicationException($"Failed to write to bid log file: {ex.Message}", ex);
            }
        }
    }
}