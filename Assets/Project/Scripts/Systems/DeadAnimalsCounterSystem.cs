using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;
using Constants;
using Utils.ECS;

[Il2CppSetOption(Option.NullChecks, false)]
[Il2CppSetOption(Option.ArrayBoundsChecks, false)]
[Il2CppSetOption(Option.DivideByZeroChecks, false)]

public struct DeadAnimalsCounterSystemState : IReactiveState
{
    public int Epoch { get; set; }
}

public sealed class DeadAnimalsCounterSystem : ISystem
{
    public World World { get; set; } = null!;

    private Filter filter = null!;
    private Filter counterProviderFilter = null!;
    private Stash<DeadAnimalsCounterComponent> counterStash = null!;
    private Stash<AnimalComponent> animalStash = null!;

    public void OnAwake()
    {
        this.filter = this.World.Filter
            .With<DeadComponent>()
            .With<AnimalComponent>()
            .Build();

        this.counterProviderFilter = this.World.Filter.With<DeadAnimalsCounterComponent>().Build();

        this.animalStash = this.World.GetStash<AnimalComponent>();
        this.counterStash = this.World.GetStash<DeadAnimalsCounterComponent>();
    }

    public void OnUpdate(float deltaTime)
    {
        foreach (var counterEntity in this.counterProviderFilter)
        {
            ref var counter = ref this.counterStash.Get(counterEntity);

            foreach (var entity in this.filter)
            {
                if (!this.animalStash.Has(entity))
                {
                    continue;
                }

                ref var animal = ref this.animalStash.Get(entity);

                if (animal.Type == AnimalType.PREY)
                {
                    counter.DeadPreys++;
                }
                else if (animal.Type == AnimalType.PREDATOR)
                {
                    counter.DeadPredators++;
                }
            }

            counter.DeadPreysCounter.text = $"Dead preys: {counter.DeadPreys}";
            counter.DeadPredatorsCounter.text = $"Dead predators: {counter.DeadPredators}";
        }
    }

    public void Dispose() { }
}