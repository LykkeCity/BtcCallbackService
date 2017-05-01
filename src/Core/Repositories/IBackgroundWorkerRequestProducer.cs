using System.Threading.Tasks;

namespace Core.Repositories
{
    public enum WorkType
    {
        UpdateHashForOperations = 80
    }

    public interface IBackgroundWorkRequestProducer
    {
        Task ProduceRequest<T>(WorkType workType, T context);
    }

    public class UpdateHashForOperationsContext
    {
        public UpdateHashForOperationsContext(string cmdType, string contextData, string hash)
        {
            ContextData = contextData;
            CmdType = cmdType;
            Hash = hash;
        }

        public string ContextData { get; set; }
        public string Hash { get; set; }
        public string CmdType { get; set; }
    }


    public class BackgroundWorkMessage
    {
        public WorkType WorkType { get; set; }
        public string ContextJson { get; set; }
    }

    public class BackgroundWorkMessage<T> : BackgroundWorkMessage
    {
        public BackgroundWorkMessage(WorkType workType, T contextObj)
        {
            ContextJson = contextObj.ToJson();
            WorkType = workType;
        }
    }
}