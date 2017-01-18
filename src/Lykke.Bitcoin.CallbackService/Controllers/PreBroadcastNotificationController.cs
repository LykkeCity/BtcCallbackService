using System.Collections.Generic;
using System.Threading.Tasks;
using Common;
using Common.Log;
using Core.Repositories;
using Core.Services;
using Core.Services.Models;
using Microsoft.AspNetCore.Mvc;

namespace Lykke.Bitcoin.CallbackService.Controllers
{
    [Route("api/[controller]")]
    public class PreBroadcastNotificationController : Controller
    {
        private readonly IPreBroadcastHandler _preBroadcastHandler;
        private readonly ILog _log;
        private readonly IProcessedTransactionsRepository _processedTransactionsRepository;

        public PreBroadcastNotificationController(IPreBroadcastHandler preBroadcastHandler, ILog log,
            IProcessedTransactionsRepository processedTransactionsRepository)
        {
            _preBroadcastHandler = preBroadcastHandler;
            _log = log;
            _processedTransactionsRepository = processedTransactionsRepository;
        }

        /// <summary>
        /// Used to notify LykkeWallet Backend before transaction broadcasting
        /// </summary>
        [HttpPost]
        public async Task Post([FromBody]TransactionNotification transactionNotification)
        {
            var logTasks = new List<Task>
            {
                _log.WriteInfoAsync("PreBroadcastNotificationController", "Post", transactionNotification.ToJson(),
                    "In method")
            };


            var canHandle =
                await _processedTransactionsRepository.CanStartPreBroadcast(transactionNotification.TransactionId);

            if (canHandle)
            {
                await _preBroadcastHandler.HandleNotification(transactionNotification);
            }
            else
            {
                logTasks.Add(
                    _log.WriteInfoAsync("PreBroadcastNotificationController", "Post",
                        transactionNotification.ToJson(),
                        "Already processed. Skipped.")
                );
            }

            await Task.WhenAll(logTasks);
        }
    }
}
