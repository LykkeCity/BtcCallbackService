using System;
using System.Threading.Tasks;
using AzureStorage;
using Core.Repositories;
using Microsoft.WindowsAzure.Storage.Table;

namespace AzureRepositories
{
    public class InternalTransactionEntity : TableEntity, IInternalTransaction
    {
        public static string GeneratePartitionKey(string hash)
        {
            return hash;
        }

        public static string GenerateRowKey(Guid transaction)
        {
            return transaction.ToString();
        }

        public static InternalTransactionEntity Create(IInternalTransaction transaction)
        {
            return new InternalTransactionEntity
            {
                RowKey = GenerateRowKey(transaction.TransactionId),
                PartitionKey = GeneratePartitionKey(transaction.Hash)
            };
        }

        public Guid TransactionId => Guid.Parse(RowKey);
        public string Hash => PartitionKey;
    }

    public class InternalTransactionsRepository : IInternalTransactionsRepository
    {
        private readonly INoSQLTableStorage<InternalTransactionEntity> _tableStorage;

        public InternalTransactionsRepository(INoSQLTableStorage<InternalTransactionEntity> tableStorage)
        {
            _tableStorage = tableStorage;
        }

        public async Task InsertAsync(IInternalTransaction transaction)
        {
            var entity = InternalTransactionEntity.Create(transaction);
            await _tableStorage.InsertAsync(entity);
        }

        public async Task<bool> IsInternalTransaction(string hash)
        {
            var record = await _tableStorage.GetDataAsync(InternalTransactionEntity.GeneratePartitionKey(hash));
            return record != null;
        }
    }
}
