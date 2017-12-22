using System.Threading.Tasks;
using AzureStorage.Queue;
using Common.Log;
using Core;
using Core.Repositories;

namespace AzureRepositories
{
    public class HashEventQueueSender : IHashEventQueueSender
    {
        private readonly IQueueExt _queueExt;
        private readonly ILog _log;

        public HashEventQueueSender(IQueueExt queueExt, ILog log)
        {
            _queueExt = queueExt;
            _log = log;
        }

        public async Task Send(string id, string hash)
        {
            var logTask = _log.WriteInfoAsync("HashEventQueueSender", "Send", $"id: {id}, hash: {hash}", "Sending msg");

            await _queueExt.PutRawMessageAsync(new HashEvent
            {
                Id = id,
                Hash = hash
            }.ToJson());

            await logTask;
        }
    }

    public class HashEvent
    {
        public string Id { get; set; }
        public string Hash { get; set; }
    }
}
