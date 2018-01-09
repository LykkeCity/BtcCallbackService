using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Core.Services.Models;

namespace Core.Services
{
    public interface IHandler
    {
        Task HandleNotification(TransactionNotification notification);

        Task HandleAggregatedCashout(List<Guid> id, string hash);
    }
}
