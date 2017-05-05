using Common.Log;
using Core.PerformanceMonitor;

namespace Services.PerformanceMonitor
{
    public class PerformanceMonitorFactory : IPerformanceMonitorFactory
    {
        private readonly ILog _logger;

        public PerformanceMonitorFactory(ILog logger)
        {
            _logger = logger;
        }

        public IPerformanceMonitor Create(string topProcess)
        {
            var monitor = new PerformanceMonitor(_logger);
            monitor.Start(topProcess);
            return monitor;
        }
    }
}
