using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Core.Repositories
{
    public interface IBitcoinTransactionContextBlobStorage
    {
        Task<string> Get(string transactionId);
        Task Set(string transactionId, string context);
    }
}
