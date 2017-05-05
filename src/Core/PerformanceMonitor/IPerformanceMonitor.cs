using System;

namespace Core.PerformanceMonitor
{
    public interface IPerformanceMonitor : IDisposable
    {       
        void Step(string nextStep);
        void ChildProcess(string childProcess);
        void Complete(string process);
        void CompleteLastProcess();
    }
}
