using System;
using System.Threading.Tasks;
using AzureStorage;
using Core.Repositories;
using Microsoft.WindowsAzure.Storage.Table;

namespace AzureRepositories
{
    public class PreProcessedTransaction : TableEntity
    {
        public string TransactionId => RowKey;

        public static PreProcessedTransaction Create(Guid transactionId)
        {
            return new PreProcessedTransaction
            {
                RowKey = transactionId.ToString(),
                PartitionKey = "Pre"
            };
        }
    }

    public class PostProcessedTransaction : TableEntity
    {
        public string TransactionId => RowKey;

        public static PostProcessedTransaction Create(Guid transactionId)
        {
            return new PostProcessedTransaction
            {
                RowKey = transactionId.ToString(),
                PartitionKey = "Post"
            };
        }
    }

    public class ProcessedTransactionsRepository : IProcessedTransactionsRepository
    {
        private readonly INoSQLTableStorage<PreProcessedTransaction> _preTableStorage;
        private readonly INoSQLTableStorage<PostProcessedTransaction> _postTableStorage;

        public ProcessedTransactionsRepository(INoSQLTableStorage<PreProcessedTransaction> preTableStorage,
            INoSQLTableStorage<PostProcessedTransaction> postTableStorage)
        {
            _preTableStorage = preTableStorage;
            _postTableStorage = postTableStorage;
        }

        public async Task<bool> CanStartPreBroadcast(Guid transactionId)
        {
            try
            {
                var entity = PreProcessedTransaction.Create(transactionId);
                await _preTableStorage.InsertAsync(entity, AzureStorageUtils.Conflict);
                return true;
            }
            catch
            {
                // ignored
            }
            return false;
        }

        public async Task<bool> CanStartPostBroadcast(Guid transactionId)
        {
            try
            {
                var entity = PostProcessedTransaction.Create(transactionId);
                await _postTableStorage.InsertAsync(entity, AzureStorageUtils.Conflict);
                return true;
            }
            catch
            {
                // ignored
            }
            return false;
        }
    }
}
