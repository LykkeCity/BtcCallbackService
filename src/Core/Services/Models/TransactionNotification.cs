using System;

namespace Core.Services.Models
{
    public class TransactionNotification
    {
        /// <summary>
        /// Internal transaction id
        /// </summary>
        public Guid TransactionId { get; set; }

        /// <summary>
        /// Blockchain tx hash
        /// </summary>
        public string TransactionHash { get; set; }
    }
}
