using Scellecs.Morpeh;

using Unity.IL2CPP.CompilerServices;

[Il2CppSetOption(Option.NullChecks, false)]
[Il2CppSetOption(Option.ArrayBoundsChecks, false)]
[Il2CppSetOption(Option.DivideByZeroChecks, false)]
public sealed class LinearMoveSystem : ISystem
{
    public World World { get; set; } = null!;

    private Filter filter = null!;
    private Stash<LinearMoveComponent> moveStash = null!;
    private Stash<PhysicBodyComponent> bodyStash = null!;

    public void OnAwake()
    {
        this.filter = this.World.Filter.With<LinearMoveComponent>().With<PhysicBodyComponent>().Build();
        this.moveStash = this.World.GetStash<LinearMoveComponent>();
        this.bodyStash = this.World.GetStash<PhysicBodyComponent>();
    }

    public void OnUpdate(float deltaTime)
    {
        foreach (var entity in this.filter)
        {
            ref var move = ref this.moveStash.Get(entity);
            ref var body = ref this.bodyStash.Get(entity);

            if (body.Body == null)
                continue;

            body.Body.MovePosition(
                body.Body.position + (deltaTime * move.Speed * move.Direction.normalized)
            );
        }
    }

    public void Dispose() { }
}