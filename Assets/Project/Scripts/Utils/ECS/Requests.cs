using Scellecs.Morpeh;

using System;

namespace Utils.ECS
{
    public struct Request<T> : IComponent where T : struct
    {
        public T Value;
        public int Accepted;
    }

    public sealed class RequestSender<T> where T : struct
    {
        private readonly World world;
        private readonly Stash<Request<T>> requestStash;

        public RequestSender(World world)
        {
            this.world = world;
            this.requestStash = this.world.GetStash<Request<T>>();
        }

        public Entity Send(in T value)
        {
            var entity = this.world.CreateEntity();
            ref var data = ref this.requestStash.Add(entity);
            data.Value = value;
            data.Accepted = 0;

            return entity;
        }

        /// <summary>
        /// <remarks>Experimental.</remarks>
        /// Sends a request to the specified target entity.
        /// There should be only one request of the same type per target entity.
        /// </summary>
        public Entity SendTo(Entity target, in T value)
        {
            ref var data = ref this.requestStash.Add(target);
            data.Value = value;
            data.Accepted = 0;

            return target;
        }
    }

    public sealed class RequestReciver<T> where T : struct
    {
        private readonly World world;
        private readonly Filter filter;
        private readonly Stash<Request<T>> stash;

        public RequestReciver(World world)
        {
            this.world = world;
            this.stash = this.world.GetStash<Request<T>>();
            this.filter = this.world.Filter.With<Request<T>>().Build();
        }

        public void ForEachAccept(Action<T, Entity> handler)
        {
            foreach (var entity in this.filter)
            {
                this.AcceptRequest(entity, handler);
            }
        }

        private T AcceptRequest(Entity entity, Action<T, Entity>? handler = null)
        {
            ref var request = ref this.stash.Get(entity);
            if (request.Accepted != 0)
            {
                return request.Value;
            }
            request.Accepted = 1;
            handler?.Invoke(request.Value, entity);
            this.stash.Remove(entity);

            return request.Value;
        }
    }
}
