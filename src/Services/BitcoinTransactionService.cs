using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Core;
using Core.Repositories;
using Core.Services;
using Core.Services.Models;

namespace Services
{
    public class BitcoinTransactionService : IBitcoinTransactionService
    {
        private readonly IBitCoinTransactionsRepository _bitCoinTransactionsRepository;
        private readonly IBitcoinTransactionContextBlobStorage _contextBlobStorage;

        public BitcoinTransactionService(IBitCoinTransactionsRepository bitCoinTransactionsRepository, IBitcoinTransactionContextBlobStorage contextBlobStorage)
        {
            _bitCoinTransactionsRepository = bitCoinTransactionsRepository;
            _contextBlobStorage = contextBlobStorage;
        }

        public async Task<T> GetTransactionContext<T>(string transactionId) where T : BaseContextData
        {
            var fromBlob = await _contextBlobStorage.Get(transactionId);
            if (string.IsNullOrWhiteSpace(fromBlob))
            {
                var transaction = await _bitCoinTransactionsRepository.FindByTransactionIdAsync(transactionId);
                fromBlob = transaction?.ContextData;
            }

            if (fromBlob == null)
                return default(T);

            return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(fromBlob);
        }

        public Task SetTransactionContext<T>(string transactionId, T context) where T : BaseContextData
        {
            return _contextBlobStorage.Set(transactionId, context.ToJson());
        }

        public Task SetStringTransactionContext(string transactionId, string context)
        {
            return _contextBlobStorage.Set(transactionId, context);
        }
    }
}
