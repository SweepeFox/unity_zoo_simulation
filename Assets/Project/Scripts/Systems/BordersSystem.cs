using Scellecs.Morpeh;

using Unity.IL2CPP.CompilerServices;

using UnityEngine;

[Il2CppSetOption(Option.NullChecks, false)]
[Il2CppSetOption(Option.ArrayBoundsChecks, false)]
[Il2CppSetOption(Option.DivideByZeroChecks, false)]
public sealed class BordersSystem : ISystem
{
    public World World { get; set; } = null!;
    private Filter linearFilter = null!;
    private Filter jumpFilter = null!;
    private Stash<LinearMoveComponent> linearMoveStash = null!;
    private Stash<JumpMoveComponent> jumpMoveStash = null!;

    private Stash<PhysicBodyComponent> physicBodyComponentStash = null!;

    public void OnAwake()
    {
        this.linearFilter = this.World.Filter.With<LinearMoveComponent>().With<PhysicBodyComponent>().Build();
        this.jumpFilter = this.World.Filter.With<JumpMoveComponent>().With<PhysicBodyComponent>().Build();

        this.linearMoveStash = this.World.GetStash<LinearMoveComponent>();
        this.jumpMoveStash = this.World.GetStash<JumpMoveComponent>();

        this.physicBodyComponentStash = this.World.GetStash<PhysicBodyComponent>();
    }

    public void OnUpdate(float deltaTime)
    {
        foreach (var entity in this.linearFilter)
        {
            ref var move = ref this.linearMoveStash.Get(entity);
            ref var body = ref this.physicBodyComponentStash.Get(entity);
            if (body.Body != null)
            {
                this.HandleBorders(ref move.Direction, body.Body);
            }
        }

        foreach (var entity in this.jumpFilter)
        {
            ref var move = ref this.jumpMoveStash.Get(entity);
            ref var body = ref this.physicBodyComponentStash.Get(entity);
            if (body.Body != null)
            {
                this.HandleBorders(ref move.Direction, body.Body);
            }
        }
    }

    private void HandleBorders(ref Vector3 direction, Rigidbody body)
    {
        if (body == null)
            return;

        Vector3 viewportPos = Camera.main.WorldToViewportPoint(body.position);

        if (viewportPos.x < 0f)
        {
            direction = this.GetRandomDirection(Vector3.right);
        }
        else if (viewportPos.x > 1f)
        {
            direction = this.GetRandomDirection(Vector3.left);
        }

        if (viewportPos.y < 0f)
        {
            direction = this.GetRandomDirection(Vector3.up);
        }
        else if (viewportPos.y > 1f)
        {
            direction = this.GetRandomDirection(Vector3.down);
        }
    }


    private Vector3 GetRandomDirection(Vector3 baseDir)
    {
        var random = new Vector3
        (
            x: baseDir.x + Random.Range(-0.5f, 0.5f),
            y: 0f,
            z: baseDir.z + Random.Range(-0.5f, 0.5f)
        );
        return random.normalized;
    }

    public void Dispose() { }
}