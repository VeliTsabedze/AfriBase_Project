namespace AfriBase.Models
{
    /// <summary>
    /// View model for the error page
    /// </summary>
    public class ErrorViewModel
    {
        /// <summary>
        /// ID of the request that caused the error
        /// </summary>
        public string RequestId { get; set; }

        /// <summary>
        /// Flag indicating if the request ID should be shown
        /// </summary>
        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }
}