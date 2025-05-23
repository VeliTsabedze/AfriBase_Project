using AfriBase.Models;
using System.Threading.Tasks;

namespace AfriBase.Services
{
    /// <summary>
    /// Interface for the bid logging service
    /// </summary>
    public interface IBidLogService
    {
        /// <summary>
        /// Log a new bid
        /// </summary>
        /// <param name="bid">The bid to log</param>
        /// <param name="artifactName">The name of the artifact being bid on</param>
        /// <returns>Task</returns>
        Task LogBidAsync(Bid bid, string artifactName);

        /// <summary>
        /// Log the acceptance of a bid
        /// </summary>
        /// <param name="bid">The bid that was accepted</param>
        /// <returns>Task</returns>
        Task LogBidAcceptanceAsync(Bid bid);

        /// <summary>
        /// Log the rejection of a bid
        /// </summary>
        /// <param name="bid">The bid that was rejected</param>
        /// <returns>Task</returns>
        Task LogBidRejectionAsync(Bid bid);

        /// <summary>
        /// Log the cancellation of a bid
        /// </summary>
        /// <param name="bid">The bid that was cancelled</param>
        /// <returns>Task</returns>
        Task LogBidCancellationAsync(Bid bid);
    }
}