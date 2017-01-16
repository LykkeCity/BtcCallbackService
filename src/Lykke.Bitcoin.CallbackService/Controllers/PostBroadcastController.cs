using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common;
using Common.Log;
using Core.Services;
using Core.Services.Models;
using Microsoft.AspNetCore.Mvc;

namespace Lykke.Bitcoin.CallbackService.Controllers
{
    [Route("api/[controller]")]
    public class PostBroadcastNotificationController : Controller
    {
        private readonly IPostBroadcastHandler _postBroadcastHandler;
        private readonly ILog _log;

        public PostBroadcastNotificationController(IPostBroadcastHandler postBroadcastHandler, ILog log)
        {
            _postBroadcastHandler = postBroadcastHandler;
            _log = log;
        }

        /// <summary>
        /// Used to notify LykkeWallet Backend before transaction broadcasting
        /// </summary>
        [HttpPost]
        public async Task Post([FromBody]TransactionNotification transactionNotification)
        {
            var log = _log.WriteInfoAsync("PostBroadcastNotificationController", "Post",
                transactionNotification.ToJson(),
                "In method");

            await _postBroadcastHandler.HandleNotification(transactionNotification);

            await log;
        }
    }
}
