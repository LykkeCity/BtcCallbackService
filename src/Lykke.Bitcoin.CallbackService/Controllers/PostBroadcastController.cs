using System.Collections.Generic;
using System.Threading.Tasks;
using Common;
using Common.Log;
using Core.PerformanceMonitor;
using Core.Repositories;
using Core.Services;
using Core.Services.Models;
using Microsoft.AspNetCore.Mvc;

namespace Lykke.Bitcoin.CallbackService.Controllers
{
    [Route("api/[controller]")]
    public class PostBroadcastNotificationController : Controller
    {
        private readonly IPostBroadcastHandler _postBroadcastHandler;

        public PostBroadcastNotificationController(IPostBroadcastHandler postBroadcastHandler)
        {
            _postBroadcastHandler = postBroadcastHandler;
        }

        /// <summary>
        /// Used to notify LykkeWallet Backend before transaction broadcasting
        /// </summary>
        [HttpPost]
        public async Task Post([FromBody] TransactionNotification transactionNotification)
        {   
            await _postBroadcastHandler.HandleNotification(transactionNotification);
        }
    }
}
