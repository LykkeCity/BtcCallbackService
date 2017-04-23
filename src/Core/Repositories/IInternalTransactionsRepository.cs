using System;
using System.Threading.Tasks;

namespace Core.Repositories
{
    public interface IInternalTransaction
    {
        Guid TransactionId { get; }
        string Hash { get; }
    }

    public class InternalTransaction : IInternalTransaction
    {
        public Guid TransactionId { get; set; }
        public string Hash { get; set; }
    }

    public interface IInternalTransactionsRepository
    {
        Task InsertAsync(IInternalTransaction transaction);
        Task<bool> IsInternalTransaction(string hash);
    }
}
