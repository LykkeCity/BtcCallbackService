using System.Threading.Tasks;
using AzureStorage.Queue;
using Common.Log;
using Core.Repositories;

namespace AzureRepositories
{
    public class TransactionQueueSender : ITransactionQueueSender
    {
        private readonly IQueueExt _queueExt;
        private readonly ILog _log;

        public TransactionQueueSender(IQueueExt queueExt, ILog log)
        {
            _queueExt = queueExt;
            _log = log;
        }

        public async Task Send(string command, string id, string hash)
        {
            var msg =
                $"{command}:{{\"TransactionId\":\"{id}\",\"Result\":{{ \"TransactionHex\":\"-\",\"TransactionHash\":\"{hash}\"}},\"Error\":null }}";

            var logTask = _log.WriteInfoAsync("TransactionQueueSender", "Send", msg, "Sending msg");

            await
                _queueExt.PutRawMessageAsync(msg);

            await logTask;
        }
    }
}
