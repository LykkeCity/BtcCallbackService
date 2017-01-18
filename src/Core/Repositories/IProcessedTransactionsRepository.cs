using System;
using System.Threading.Tasks;

namespace Core.Repositories
{
    public interface IProcessedTransactionsRepository
    {
        Task<bool> CanStartPreBroadcast(Guid transactionId);
        Task<bool> CanStartPostBroadcast(Guid transactionId);
    }
}
