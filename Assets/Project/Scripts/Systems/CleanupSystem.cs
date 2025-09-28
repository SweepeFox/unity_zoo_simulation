using Scellecs.Morpeh;

using Unity.IL2CPP.CompilerServices;

using UnityEngine;

[Il2CppSetOption(Option.NullChecks, false)]
[Il2CppSetOption(Option.ArrayBoundsChecks, false)]
[Il2CppSetOption(Option.DivideByZeroChecks, false)]
public sealed class CleanupSystem : ISystem
{
    public World World { get; set; } = null!;
    private Filter filter = null!;
    private Stash<PhysicBodyComponent> bodyStash = null!;

    public void OnAwake()
    {
        this.filter = this.World.Filter.With<DeadComponent>().With<PhysicBodyComponent>().Build();
        this.bodyStash = this.World.GetStash<PhysicBodyComponent>();
    }

    public void OnUpdate(float deltaTime)
    {
        foreach (var entity in this.filter)
        {
            if (this.bodyStash.Has(entity))
            {
                ref var body = ref this.bodyStash.Get(entity);
                if (body.Body != null)
                {
                    Object.Destroy(body.Body.gameObject);
                }
            }

            this.World.RemoveEntity(entity);
        }
    }

    public void Dispose() { }
}
