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
        private readonly IBackgroundWorkRequestProducer _backgroundWorkRequestProducer;
        private readonly IPerformanceMonitorFactory _performanceMonitorFactory;

        public PreBroadcastHandler(IBitCoinTransactionsRepository bitCoinTransactionsRepository,
            IInternalOperationsRepository internalOperationsRepository,
            IBackgroundWorkRequestProducer backgroundWorkRequestProducer, IPerformanceMonitorFactory performanceMonitorFactory)
        {
            _bitCoinTransactionsRepository = bitCoinTransactionsRepository;
            _internalOperationsRepository = internalOperationsRepository;
            _backgroundWorkRequestProducer = backgroundWorkRequestProducer;
            _performanceMonitorFactory = performanceMonitorFactory;
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
                    OperationIds = GetOperationIds(tx)
                });

                monitor.Step("Produce request");
                if (tx != null)
                {
                    var cmdType = tx.CommandType;

                    await _backgroundWorkRequestProducer.ProduceRequest(
                        WorkType.UpdateHashForOperations, new UpdateHashForOperationsContext(cmdType, tx.ContextData, notification.TransactionHash));
                }

                monitor.Step("Wait for insert internal operation");
                await insertInternalOperationTask;
            }
        }

        private string[] GetOperationIds(IBitcoinTransaction tx)
        {
            switch (tx?.CommandType)
            {
                case CommandTypes.Issue:
                    var issueContext = tx.ContextData.DeserializeJson<IssueContextData>();
                    return new[] { issueContext.CashOperationId };
                case CommandTypes.CashOut:
                    var cashOutContext = tx.ContextData.DeserializeJson<CashOutContextData>();
                    return new[] { cashOutContext.CashOperationId };
                case CommandTypes.Swap:
                    var swapContext = tx.ContextData.DeserializeJson<SwapContextData>();
                    return swapContext.Trades.Select(x => x.TradeId).ToArray();
                case CommandTypes.Transfer:
                    var transferContext = tx.ContextData.DeserializeJson<TransferContextData>();
                    return transferContext.Transfers.Select(x => x.OperationId).ToArray();
                case CommandTypes.TransferAll:
                    var transferAllContext = tx.ContextData.DeserializeJson<TransferContextData>();
                    return transferAllContext.Transfers.Select(x => x.OperationId).ToArray();
                case CommandTypes.Destroy:
                    var destroyContext = tx.ContextData.DeserializeJson<UncolorContextData>();
                    return new[] { destroyContext.CashOperationId };
            }

            return null;
        }
    }
}
