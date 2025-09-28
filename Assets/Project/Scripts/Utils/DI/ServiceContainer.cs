using Microsoft.Extensions.DependencyInjection;

using Scellecs.Morpeh;

using System;

namespace Utils.DI
{
    public interface IServiceContainer
    {
        IServiceProvider ServiceProvider { get; }

        ISystem CreateSystem<T>(params object[] parameters) where T : ISystem;
    }

    public class ServiceContainer : IServiceContainer
    {
        public IServiceProvider ServiceProvider { get; private set; }

        public ServiceContainer(ServiceCollection serviceCollection)
        {
            this.ServiceProvider = serviceCollection.BuildServiceProvider();
        }

        public ISystem CreateSystem<T>(params object[] parameters) where T : ISystem
        {
            return ActivatorUtilities.CreateInstance<T>(this.ServiceProvider, parameters);
        }
    }
}