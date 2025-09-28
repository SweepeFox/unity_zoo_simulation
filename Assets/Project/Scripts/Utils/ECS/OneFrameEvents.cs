using Scellecs.Morpeh;

using System;

namespace Utils.ECS
{
    public struct OneFrameEvent<T> : IComponent where T : struct
    {
        public T Value;
    }

    public class OneFrameEventsEmitter<T> where T : struct
    {
        private readonly World world;
        private readonly Stash<OneFrameEvent<T>> stash;

        public OneFrameEventsEmitter(World world)
        {
            this.world = world;
            this.stash = this.world.GetStash<OneFrameEvent<T>>();
        }

        public Entity Emit(in T value)
        {
            var entity = this.world.CreateEntity();
            ref var data = ref this.stash.Add(entity);
            data.Value = value;

            return entity;
        }

        /// <summary>
        /// <remarks>Experimental.</remarks>
        /// Emits an one frame event to the specified target entity.
        /// There should be only one event of the same type per target entity.
        /// </summary>
        public Entity EmitTo(Entity target, in T value)
        {
            ref var data = ref this.stash.Add(target);
            data.Value = value;

            return target;
        }
    }

    public sealed class OneFrameEventReader<T> where T : struct
    {
        private readonly World world;
        private readonly Filter filter;
        private readonly Stash<OneFrameEvent<T>> eventStash;

        public OneFrameEventReader(World world)
        {
            this.world = world;
            this.eventStash = this.world.GetStash<OneFrameEvent<T>>();
            this.filter = this.world.Filter.With<OneFrameEvent<T>>().Build();
        }

        public bool TryGetFrom(Entity entity, out T value)
        {
            if (this.world.IsDisposed(entity) || !this.eventStash.Has(entity))
            {
                value = default;
                return false;
            }
            ref var e = ref this.eventStash.Get(entity);
            value = e.Value;
            return true;
        }

        public void ForEach(Action<T, Entity> handler)
        {
            foreach (var entity in this.filter)
            {
                ref var e = ref this.eventStash.Get(entity);
                handler(e.Value, entity);
            }
        }
    }
}
