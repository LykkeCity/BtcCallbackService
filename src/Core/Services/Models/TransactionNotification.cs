namespace Core.Services.Models
{
    public class TransactionNotification
    {
        /// <summary>
        /// Internal transaction id
        /// </summary>
        public string TransactionId { get; set; }

        /// <summary>
        /// Blockchain tx hash
        /// </summary>
        public string TransactionHash { get; set; }
    }
}
