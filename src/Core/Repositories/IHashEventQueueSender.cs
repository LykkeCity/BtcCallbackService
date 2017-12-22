using System.Threading.Tasks;

namespace Core.Repositories
{
    public interface IHashEventQueueSender
    {
        Task Send(string id, string hash);
    }
}
