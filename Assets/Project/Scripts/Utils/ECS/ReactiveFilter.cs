using Scellecs.Morpeh;

namespace Utils.ECS
{
    public interface IReactiveState : IComponent
    {
        int Epoch { get; set; }
    }

    public sealed class ReactiveFilter<TState>
        where TState : struct, IReactiveState
    {
        public delegate void AddDelegate(Entity entity, ref TState state);
        public delegate void UpdateDelegate(Entity entity, ref TState state, float dt);
        public delegate void RemoveDelegate(Entity entity, ref TState state);

        public AddDelegate? OnAdd { get; set; }
        public UpdateDelegate? OnUpdate { get; set; }
        public RemoveDelegate? OnRemove { get; set; }

        private readonly World world;
        private readonly Filter filter;
        private readonly Filter statesOnly;
        private readonly Stash<TState> stateStash;

        private int epoch;

        public ReactiveFilter(World world, Filter filter, AddDelegate? onAdd = null, UpdateDelegate? onUpdate = null, RemoveDelegate? onRemove = null)
        {
            this.world = world;
            this.filter = filter;
            this.statesOnly = world.Filter.With<TState>().Build();
            this.stateStash = world.GetStash<TState>();

            this.OnAdd = onAdd;
            this.OnUpdate = onUpdate;
            this.OnRemove = onRemove;
        }

        public void Process(float deltaTime)
        {
            if (this.epoch == int.MaxValue)
            {
                this.epoch = 0;
                foreach (Entity entity in this.statesOnly)
                {
                    ref TState state = ref this.stateStash.Get(entity);
                    state.Epoch = 0;
                }
            }

            this.epoch++;

            var onAdd = this.OnAdd;
            var onUpdate = this.OnUpdate;
            var onRemove = this.OnRemove;

            foreach (Entity entity in this.filter)
            {
                if (!this.stateStash.Has(entity))
                {
                    ref TState newState = ref this.stateStash.Add(entity);
                    newState.Epoch = this.epoch;
                    onAdd?.Invoke(entity, ref newState);
                }
                else
                {
                    ref TState state = ref this.stateStash.Get(entity);
                    state.Epoch = this.epoch;
                    onUpdate?.Invoke(entity, ref state, deltaTime);
                }
            }

            foreach (Entity entity in this.statesOnly)
            {
                ref TState state = ref this.stateStash.Get(entity);
                if (state.Epoch != this.epoch)
                {
                    onRemove?.Invoke(entity, ref state);
                    this.stateStash.Remove(entity);
                }
            }
        }
    }
}
