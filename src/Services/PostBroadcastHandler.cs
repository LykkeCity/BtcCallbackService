using System.Threading.Tasks;
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

        public PostBroadcastHandler(ITransactionQueueSender transactionQueueSender,
            IBitCoinTransactionsRepository bitCoinTransactionsRepository)
        {
            _transactionQueueSender = transactionQueueSender;
            _bitCoinTransactionsRepository = bitCoinTransactionsRepository;
        }

        public async Task HandleNotification(TransactionNotification notification)
        {
            var tx = await _bitCoinTransactionsRepository.FindByTransactionIdAsync(notification.TransactionId.ToString());

            if (tx != null)
            {
                var cmdType = tx.RequestData.GetCommandType();
                await _transactionQueueSender.Send(cmdType, notification.TransactionId.ToString(), notification.TransactionHash);
            }
        }
    }
}
