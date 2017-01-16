namespace Core.Settings
{
    public class BaseSettings
    {
        public DbSettings Db { get; set; }
    }

    public class DbSettings
    {
        public string LogsConnString { get; set; }
        public string ClientPersonalInfoConnString { get; set; }
        public string BitCoinQueueConnectionString { get; set; }
        public string HTradesConnString { get; set; }
    }
}
