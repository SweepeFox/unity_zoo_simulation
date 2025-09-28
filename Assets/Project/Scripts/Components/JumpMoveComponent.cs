using Scellecs.Morpeh;

using Unity.IL2CPP.CompilerServices;

using UnityEngine;

[System.Serializable]
[Il2CppSetOption(Option.NullChecks, false)]
[Il2CppSetOption(Option.ArrayBoundsChecks, false)]
[Il2CppSetOption(Option.DivideByZeroChecks, false)]
public struct JumpMoveComponent : IComponent
{
    public Vector3 Direction;
    public float JumpInterval;
    public float JumpDistance;

    public float Timer;
    public bool IsJumping;
    public float JumpProgress;
    public Vector3 JumpStart;
    public Vector3 JumpTarget;
}