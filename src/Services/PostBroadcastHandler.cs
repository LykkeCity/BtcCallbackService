using System.Threading.Tasks;
using Core.PerformanceMonitor;
using Core.Repositories;
using Core.Services;
using Core.Services.Models;

namespace Services
{
    //ToDo: remove and move all logic to TxDetector. Currently it is a proxy that send to old outdata (to use old code in SrvQueueHandler)
    public class PostBroadcastHandler : IPostBroadcastHandler
    {
        private readonly ITransactionQueueSender _transactionQueueSender;
        private readonly IBitCoinTransactionsRepository _bitCoinTransactionsRepository;
        private readonly IPerformanceMonitorFactory _performanceMonitorFactory;

        public PostBroadcastHandler(ITransactionQueueSender transactionQueueSender,
            IBitCoinTransactionsRepository bitCoinTransactionsRepository, IPerformanceMonitorFactory performanceMonitorFactory)
        {
            _transactionQueueSender = transactionQueueSender;
            _bitCoinTransactionsRepository = bitCoinTransactionsRepository;
            _performanceMonitorFactory = performanceMonitorFactory;
        }

        public async Task HandleNotification(TransactionNotification notification)
        {
            using (var monitor = _performanceMonitorFactory.Create("Post broadcast"))
            {
                monitor.Step("Find transaction by id");
                var tx = await _bitCoinTransactionsRepository.FindByTransactionIdAsync(notification.TransactionId.ToString());

                monitor.Step("Send to queue");
                if (tx != null)
                {
                    var cmdType = tx.CommandType;
                    await _transactionQueueSender.Send(cmdType, notification.TransactionId.ToString(), notification.TransactionHash);
                } 
            }
        }
    }
}
