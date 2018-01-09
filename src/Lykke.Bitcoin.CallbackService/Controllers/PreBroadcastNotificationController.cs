using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common;
using Common.Log;
using Core.PerformanceMonitor;
using Core.Repositories;
using Core.Services;
using Core.Services.Models;
using Lykke.Bitcoin.CallbackService.Models;
using Microsoft.AspNetCore.Mvc;

namespace Lykke.Bitcoin.CallbackService.Controllers
{
    [Route("api/[controller]")]
    public class PreBroadcastNotificationController : Controller
    {
        private readonly IPreBroadcastHandler _preBroadcastHandler;

        public PreBroadcastNotificationController(IPreBroadcastHandler preBroadcastHandler)
        {
            _preBroadcastHandler = preBroadcastHandler;
        }

        /// <summary>
        /// Used to notify LykkeWallet Backend before transaction broadcasting
        /// </summary>
        [HttpPost]
        public async Task Post([FromBody]TransactionNotification transactionNotification)
        {
            await _preBroadcastHandler.HandleNotification(transactionNotification);
        }

        /// <summary>
        /// Used to notify LykkeWallet Backend after aggregated cashout broadcasting
        /// </summary>
        [HttpPost("aggregatedCashout")]
        public async Task Post([FromBody]AggregatedCashoutModel aggregatedCashoutNotification)
        {
            await _preBroadcastHandler.HandleAggregatedCashout(aggregatedCashoutNotification.TransactionIds, aggregatedCashoutNotification.TransactionHash);
        }
    }
}
