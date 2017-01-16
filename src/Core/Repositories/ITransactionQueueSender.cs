using System.Threading.Tasks;

namespace Core.Repositories
{
    public interface ITransactionQueueSender
    {
        Task Send(string command, string id, string hash);
    }
}
