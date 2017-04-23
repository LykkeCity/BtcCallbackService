using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Core.Repositories;
using Core.Services;
using Core.Services.Models;

namespace Services
{
    public class PreBroadcastHandler : IPreBroadcastHandler
    {
        private readonly IBitCoinTransactionsRepository _bitCoinTransactionsRepository;
        private readonly ICashOperationsRepository _cashOperationsRepository;
        private readonly IClientTradesRepository _clientTradesRepository;
        private readonly ITransferEventsRepository _transferEventsRepository;
        private readonly IInternalTransactionsRepository _internalTransactionsRepository;
        private readonly Dictionary<string, Func<IBitcoinTransaction, string, Task>> _handlers = new Dictionary<string, Func<IBitcoinTransaction, string, Task>>();

        public PreBroadcastHandler(IBitCoinTransactionsRepository bitCoinTransactionsRepository,
            ICashOperationsRepository cashOperationsRepository,
            IClientTradesRepository clientTradesRepository,
            ITransferEventsRepository transferEventsRepository, IInternalTransactionsRepository internalTransactionsRepository)
        {
            _bitCoinTransactionsRepository = bitCoinTransactionsRepository;
            _cashOperationsRepository = cashOperationsRepository;
            _clientTradesRepository = clientTradesRepository;
            _transferEventsRepository = transferEventsRepository;
            _internalTransactionsRepository = internalTransactionsRepository;

            RegisterHandler(CommandTypes.Issue, HandleIssueAsync);
            RegisterHandler(CommandTypes.CashOut, HandleCashOutAsync);
            RegisterHandler(CommandTypes.Swap, HandleSwapAsync);
            RegisterHandler(CommandTypes.Transfer, HandleTransferAsync);
            RegisterHandler(CommandTypes.TransferAll, HandleTransferAsync);
            RegisterHandler(CommandTypes.Destroy, HandleDestroyAsync);
        }

        private async Task HandleDestroyAsync(IBitcoinTransaction tx, string hash)
        {
            var contextData = tx.GetContextData<UncolorContextData>();

            await _cashOperationsRepository.UpdateBlockchainHashAsync(contextData.ClientId,
                contextData.CashOperationId, hash);
        }

        private async Task HandleIssueAsync(IBitcoinTransaction tx, string hash)
        {
            var contextData = tx.GetContextData<IssueContextData>();

            await _cashOperationsRepository.UpdateBlockchainHashAsync(contextData.ClientId,
                contextData.CashOperationId, hash);
        }

        private async Task HandleCashOutAsync(IBitcoinTransaction tx, string hash)
        {
            var contextData = tx.GetContextData<CashOutContextData>();

            await _cashOperationsRepository.UpdateBlockchainHashAsync(contextData.ClientId,
                contextData.CashOperationId, hash);
        }

        private async Task HandleSwapAsync(IBitcoinTransaction tx, string hash)
        {
            var contextData = tx.GetContextData<SwapContextData>();
            foreach (var item in contextData.Trades)
            {
                await _clientTradesRepository.UpdateBlockChainHashAsync(item.ClientId, item.TradeId,
                    hash);
            }
        }

        private async Task HandleTransferAsync(IBitcoinTransaction tx, string hash)
        {
            var contextData = tx.GetContextData<TransferContextData>();

            foreach (var transfer in contextData.Transfers)
            {
                await _transferEventsRepository.UpdateBlockChainHashAsync(transfer.ClientId, transfer.OperationId, hash);
            }
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
            var tx = await _bitCoinTransactionsRepository.FindByTransactionIdAsync(transactionModel.TransactionId.ToString());

            if (tx != null)
            {
                var cmdType = tx.CommandType;

                var handler = GetHandler(cmdType);

                if (handler != null)
                {
                    await handler(tx, transactionModel.TransactionHash);
                }
            }
            else
            {
                //unknown internal command. Should be skipped by tx detector
                await _internalTransactionsRepository.InsertAsync(new InternalTransaction
                {
                    Hash = transactionModel.TransactionHash,
                    TransactionId = transactionModel.TransactionId
                });
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
