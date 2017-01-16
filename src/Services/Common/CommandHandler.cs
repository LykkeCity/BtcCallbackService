using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Core.Repositories;
using Core.Services;
using Core.Services.Models;

namespace Services.Common
{
    public class CommandHandler : IHandler
    {
        private readonly IBitCoinTransactionsRepository _bitCoinTransactionsRepository;
        private readonly Dictionary<string, Func<IBitcoinTransaction, string, Task>> _handlers = new Dictionary<string, Func<IBitcoinTransaction, string, Task>>();

        public CommandHandler(IBitCoinTransactionsRepository bitCoinTransactionsRepository)
        {
            _bitCoinTransactionsRepository = bitCoinTransactionsRepository;
        }

        public void RegisterHandler(string operation, Func<IBitcoinTransaction, string, Task> handler)
        {
            _handlers.Add(operation, handler);
        }

        public async Task HandleNotification(TransactionNotification notification)
        {
            await HandleOperation(notification);
        }

        public async Task HandleOperation(TransactionNotification transactionModel)
        {
            var tx = await _bitCoinTransactionsRepository.FindByTransactionIdAsync(transactionModel.TransactionId);

            if (tx != null)
            {
                var cmdType = tx.RequestData.GetCommandType();

                var handler = GetHandler(cmdType);

                if (handler != null)
                {
                    await handler(tx, transactionModel.TransactionHash);
                }
            }
        }

        public Func<IBitcoinTransaction, string, Task> GetHandler(string command)
        {
            if (_handlers.ContainsKey(command))
                return _handlers[command];

            return null;
        }
    }
}
