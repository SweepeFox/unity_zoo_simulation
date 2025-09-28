using Scellecs.Morpeh;

using Unity.IL2CPP.CompilerServices;

using UnityEngine;

[Il2CppSetOption(Option.NullChecks, false)]
[Il2CppSetOption(Option.ArrayBoundsChecks, false)]
[Il2CppSetOption(Option.DivideByZeroChecks, false)]
public sealed class JumpMoveSystem : ISystem
{
    public World World { get; set; } = null!;

    private Filter filter = null!;
    private Stash<JumpMoveComponent> moveStash = null!;
    private Stash<PhysicBodyComponent> bodyStash = null!;

    public void OnAwake()
    {
        this.filter = this.World.Filter.With<JumpMoveComponent>().With<PhysicBodyComponent>().Build();
        this.moveStash = this.World.GetStash<JumpMoveComponent>();
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

            if (!move.IsJumping)
            {
                move.Timer -= deltaTime;
                if (move.Timer <= 0f)
                {
                    move.Timer = move.JumpInterval;
                    move.IsJumping = true;
                    move.JumpProgress = 0f;
                    move.JumpStart = body.Body.position;
                    move.JumpTarget = body.Body.position + move.Direction.normalized * move.JumpDistance;
                }
            }
            else
            {
                move.JumpProgress += deltaTime / 0.5f;

                float t = Mathf.Clamp01(move.JumpProgress);
                Vector3 pos = Vector3.Lerp(move.JumpStart, move.JumpTarget, t);

                pos.y += Mathf.Sin(t * Mathf.PI) * 2f;

                body.Body.MovePosition(pos);

                if (t >= 1f)
                {
                    move.IsJumping = false;
                }
            }
        }
    }

    public void Dispose() { }
}