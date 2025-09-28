using Scellecs.Morpeh;

using System;
using System.Collections.Generic;

namespace Utils.ECS
{
    public struct Event<T> : IComponent where T : struct
    {
        public T Value;
    }

    public sealed class EventEmitter<T> where T : struct
    {
        private readonly World world;
        private readonly Stash<Event<T>> stash;
        private readonly List<Entity> targets;

        public EventEmitter(World world, int expectedTargetsPerFrame = 8)
        {
            this.world = world;
            this.stash = this.world.GetStash<Event<T>>();
            this.targets = new List<Entity>(expectedTargetsPerFrame);
        }

        /// <summary>
        /// Should be called before emitting events.
        /// </summary>
        public void Process()
        {
            for (int i = 0; i < this.targets.Count; i++)
            {
                var entity = this.targets[i];
                if (this.stash.Has(entity))
                {
                    this.stash.Remove(entity);
                }
            }
            this.targets.Clear();
        }

        public Entity Emit(in T value)
        {
            var entity = this.world.CreateEntity();
            ref var data = ref this.stash.Add(entity);
            data.Value = value;
            this.targets.Add(entity);

            return entity;
        }

        /// <summary>
        /// <remarks>Experimental.</remarks>
        /// Emits an event to the specified target entity.
        /// There should be only one event of the same type per target entity.
        /// </summary>
        public Entity EmitTo(Entity target, in T value)
        {
            if (this.stash.Has(target))
            {
                ref var data = ref this.stash.Get(target);
                data.Value = value;
            }
            else
            {
                ref var data = ref this.stash.Add(target);
                data.Value = value;
            }
            this.targets.Add(target);

            return target;
        }
    }

    public sealed class EventReciver<T> where T : struct
    {
        private readonly World world;
        private readonly Filter baseFilter;
        private readonly Stash<Event<T>> stash;

        public EventReciver(World world, Filter? filter = null)
        {
            this.world = world;
            this.stash = this.world.GetStash<Event<T>>();
            this.baseFilter = filter ?? this.world.Filter.With<Event<T>>().Build();
        }

        public void ForEach(Action<T, Entity> handler)
        {
            foreach (var entity in this.baseFilter)
            {
                ref var e = ref this.stash.Get(entity);
                handler(e.Value, entity);
            }
        }

        public bool TryGetFrom(Entity entity, out T value)
        {
            if (this.world.IsDisposed(entity) || !this.stash.Has(entity))
            {
                value = default;
                return false;
            }
            ref var e = ref this.stash.Get(entity);
            value = e.Value;
            return true;
        }
    }
}
