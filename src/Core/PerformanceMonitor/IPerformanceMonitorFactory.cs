namespace Core.PerformanceMonitor
{
    public interface IPerformanceMonitorFactory
    {
        IPerformanceMonitor Create(string topProcess);
    }
}
