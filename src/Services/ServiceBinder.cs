using Autofac;
using Core.PerformanceMonitor;
using Core.Services;
using Services.PerformanceMonitor;

namespace Services
{
    public static class ServiceBinder
    {
        public static void BindServices(this ContainerBuilder ioc)
        {
            ioc.RegisterType<PreBroadcastHandler>().As<IPreBroadcastHandler>().SingleInstance();
            ioc.RegisterType<PostBroadcastHandler>().As<IPostBroadcastHandler>().SingleInstance();
            ioc.RegisterType<BitcoinTransactionService>().As<IBitcoinTransactionService>().SingleInstance();

            ioc.RegisterType<PerformanceMonitorFactory>().As<IPerformanceMonitorFactory>().SingleInstance();
        }
    }
}
