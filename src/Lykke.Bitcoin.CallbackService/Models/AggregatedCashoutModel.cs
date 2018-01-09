using System;
using System.Collections.Generic;
using System.Text;

namespace Lykke.Bitcoin.CallbackService.Models
{
    public class AggregatedCashoutModel
    {
        public List<Guid> TransactionIds { get; set; }

        public string TransactionHash { get; set; }
    }
}
