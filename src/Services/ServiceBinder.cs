using Autofac;
using Core.Services;

namespace Services
{
    public static class ServiceBinder
    {
        public static void BindServices(this ContainerBuilder ioc)
        {
            ioc.RegisterType<PreBroadcastHandler>().As<IPreBroadcastHandler>().SingleInstance();
            ioc.RegisterType<PostBroadcastHandler>().As<IPostBroadcastHandler>().SingleInstance();
        }
    }
}
