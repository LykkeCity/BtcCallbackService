using System;
using System.Threading.Tasks;

namespace Core.Repositories
{
    public interface IBitcoinTransaction
    {
        string TransactionId { get; }
        DateTime Created { get; }
        DateTime? ResponseDateTime { get; }
        string CommandType { get; }
        string RequestData { get; }
        string ResponseData { get; }
        string ContextData { get; }
        string BlockchainHash { get; }
    }

    public interface IBitCoinTransactionsRepository
    {
        Task<IBitcoinTransaction> FindByTransactionIdAsync(string transactionId);
        Task<IBitcoinTransaction> SaveResponseAndHashAsync(string transactionId, string resp, string hash, DateTime? dateTime = null);
        Task UpdateRequestAsync(string transactionId, string request);
    }

    public static class BintCoinTransactionsRepositoryExt
    {
        public static T GetContextData<T>(this IBitcoinTransaction src)
        {
            return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(src.ContextData);
        }
    }
}
