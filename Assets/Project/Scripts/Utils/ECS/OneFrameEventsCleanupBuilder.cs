using Scellecs.Morpeh;

using System;
using System.Collections.Generic;

using Utils.DI;

namespace Utils.ECS
{
    public sealed class OneFrameEventsCleanupBuilder
    {

        public static OneFrameEventsCleanupBuilder For(World world, IServiceContainer serviceContainer, SystemsGroup? systemsGroup = null)
        {
            return new OneFrameEventsCleanupBuilder(systemsGroup ?? world.CreateSystemsGroup(), serviceContainer);
        }

        private readonly SystemsGroup systemsGroup;
        private readonly IServiceContainer serviceContainer;
        private readonly HashSet<Type> registeredTypes = new();

        private OneFrameEventsCleanupBuilder(SystemsGroup systemsGroup, IServiceContainer serviceContainer)
        {
            this.systemsGroup = systemsGroup;
            this.serviceContainer = serviceContainer;
        }

        public OneFrameEventsCleanupBuilder Add<T>() where T : struct
        {
            var type = typeof(T);
            if (this.registeredTypes.Add(type))
            {
                // this.systemsGroup.AddSystem(this.serviceContainer.CreateSystem<OneFrameEventsCleanupSystem<T>>());
            }
            return this;
        }

        public SystemsGroup Done()
        {
            return this.systemsGroup;
        }

        public SystemsGroup Group => this.systemsGroup;
    }
}
