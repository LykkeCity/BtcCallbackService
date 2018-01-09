using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.PerformanceMonitor;
using Core.Repositories;
using Core.Services;
using Core.Services.Models;

namespace Services
{
    public class PostBroadcastHandler : IPostBroadcastHandler
    {
        private readonly ITransactionQueueSender _transactionQueueSender;
        private readonly IBitCoinTransactionsRepository _bitCoinTransactionsRepository;
        private readonly IPerformanceMonitorFactory _performanceMonitorFactory;
        private readonly IHashEventQueueSender _hashEventQueueSender;

        public PostBroadcastHandler(ITransactionQueueSender transactionQueueSender,
            IBitCoinTransactionsRepository bitCoinTransactionsRepository, IPerformanceMonitorFactory performanceMonitorFactory, IHashEventQueueSender hashEventQueueSender)
        {
            _transactionQueueSender = transactionQueueSender;
            _bitCoinTransactionsRepository = bitCoinTransactionsRepository;
            _performanceMonitorFactory = performanceMonitorFactory;
            _hashEventQueueSender = hashEventQueueSender;
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

        public Task HandleAggregatedCashout(List<Guid> ids, string hash)
        {
            var tasks = ids.Select(x => _hashEventQueueSender.Send(x.ToString(), hash));

            return Task.WhenAll(tasks);
        }
    }
}
