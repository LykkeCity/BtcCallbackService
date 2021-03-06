﻿using System;
using System.Threading.Tasks;
using AzureStorage;
using Common;
using Core.Repositories;
using Microsoft.WindowsAzure.Storage.Table;

namespace AzureRepositories
{
    public class InternalOperationEntity : TableEntity, IInternalOperation
    {
        public static string GeneratePartitionKey(string hash)
        {
            return hash;
        }

        public static string GenerateRowKey(Guid transaction)
        {
            return transaction.ToString();
        }

        public static InternalOperationEntity Create(IInternalOperation operation)
        {
            return new InternalOperationEntity
            {
                RowKey = GenerateRowKey(operation.TransactionId),
                PartitionKey = GeneratePartitionKey(operation.Hash),
                CommandType = operation.CommandType,
                OperationIds = operation.OperationIds
            };
        }

        public Guid TransactionId => Guid.Parse(RowKey);
        public string Hash => PartitionKey;
        public string CommandType { get; set; }

        public string OperationIdsVal { get; set; }
        public string[] OperationIds
        {
            get { return OperationIdsVal.DeserializeJson<string[]>(); }
            set { OperationIdsVal = value.ToJson(); }
        }
    }

    public class InternalOperationsRepository : IInternalOperationsRepository
    {
        private readonly INoSQLTableStorage<InternalOperationEntity> _tableStorage;

        public InternalOperationsRepository(INoSQLTableStorage<InternalOperationEntity> tableStorage)
        {
            _tableStorage = tableStorage;
        }

        public async Task InsertOrReplaceAsync(IInternalOperation operation)
        {
            var entity = InternalOperationEntity.Create(operation);
            await _tableStorage.InsertOrReplaceAsync(entity);
        }
    }
}
