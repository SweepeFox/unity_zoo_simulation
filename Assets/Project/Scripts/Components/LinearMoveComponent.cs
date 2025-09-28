using Scellecs.Morpeh;

using Unity.IL2CPP.CompilerServices;

using UnityEngine;

[System.Serializable]
[Il2CppSetOption(Option.NullChecks, false)]
[Il2CppSetOption(Option.ArrayBoundsChecks, false)]
[Il2CppSetOption(Option.DivideByZeroChecks, false)]
public struct LinearMoveComponent : IComponent
{
    public Vector3 Direction;
    public float Speed;
}