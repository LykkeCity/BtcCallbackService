using System.Linq;
using System.Threading.Tasks;
using Common;
using Core.PerformanceMonitor;
using Core.Repositories;
using Core.Services;
using Core.Services.Models;

namespace Services
{
    public class PreBroadcastHandler : IPreBroadcastHandler
    {
        private readonly IBitCoinTransactionsRepository _bitCoinTransactionsRepository;
        private readonly IInternalOperationsRepository _internalOperationsRepository;
        private readonly IPerformanceMonitorFactory _performanceMonitorFactory;
        private readonly IBitcoinTransactionService _bitcoinTransactionService;
        private readonly IHashEventQueueSender _hashEventQueueSender;

        public PreBroadcastHandler(IBitCoinTransactionsRepository bitCoinTransactionsRepository,
            IInternalOperationsRepository internalOperationsRepository,
            IPerformanceMonitorFactory performanceMonitorFactory, IBitcoinTransactionService bitcoinTransactionService, IHashEventQueueSender hashEventQueueSender)
        {
            _bitCoinTransactionsRepository = bitCoinTransactionsRepository;
            _internalOperationsRepository = internalOperationsRepository;
            _performanceMonitorFactory = performanceMonitorFactory;
            _bitcoinTransactionService = bitcoinTransactionService;
            _hashEventQueueSender = hashEventQueueSender;
        }

        public async Task HandleNotification(TransactionNotification notification)
        {
            using (var monitor = _performanceMonitorFactory.Create("Prebroadcast"))
            {
                monitor.Step("Find by transactiom id");
                var tx = await _bitCoinTransactionsRepository.FindByTransactionIdAsync(notification.TransactionId.ToString());

                var insertInternalOperationTask = _internalOperationsRepository.InsertOrReplaceAsync(new InternalOperation
                {
                    Hash = notification.TransactionHash,
                    TransactionId = notification.TransactionId,
                    CommandType = tx?.CommandType,
                    OperationIds = await GetOperationIds(tx)
                });
                
                var hashEventTask = _hashEventQueueSender.Send(notification.TransactionId.ToString(), notification.TransactionHash);

                monitor.Step("Wait for insert internal operation");
                await Task.WhenAll(hashEventTask, insertInternalOperationTask);
            }
        }

        private async Task<string[]> GetOperationIds(IBitcoinTransaction tx)
        {

            switch (tx?.CommandType)
            {
                case CommandTypes.Issue:
                    var issueContext = await _bitcoinTransactionService.GetTransactionContext<IssueContextData>(tx.TransactionId);
                    return new[] { issueContext.CashOperationId };
                case CommandTypes.CashOut:
                    var cashOutContext = await _bitcoinTransactionService.GetTransactionContext<CashOutContextData>(tx.TransactionId);
                    return new[] { cashOutContext.CashOperationId };
                case CommandTypes.Swap:
                    var swapContext = await _bitcoinTransactionService.GetTransactionContext<SwapContextData>(tx.TransactionId);
                    return swapContext.Trades.Select(x => x.TradeId).ToArray();
                case CommandTypes.Transfer:
                    var transferContext = await _bitcoinTransactionService.GetTransactionContext<TransferContextData>(tx.TransactionId);
                    return transferContext.Transfers.Select(x => x.OperationId).ToArray();
                case CommandTypes.TransferAll:
                    var transferAllContext = await _bitcoinTransactionService.GetTransactionContext<TransferContextData>(tx.TransactionId);
                    return transferAllContext.Transfers.Select(x => x.OperationId).ToArray();
                case CommandTypes.Destroy:
                    var destroyContext = await _bitcoinTransactionService.GetTransactionContext<UncolorContextData>(tx.TransactionId);
                    return new[] { destroyContext.CashOperationId };
            }

            return null;
        }
    }
}
