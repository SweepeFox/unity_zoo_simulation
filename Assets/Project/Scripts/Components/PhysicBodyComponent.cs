using Scellecs.Morpeh;

using Unity.IL2CPP.CompilerServices;

using UnityEngine;

[System.Serializable]
[Il2CppSetOption(Option.NullChecks, false)]
[Il2CppSetOption(Option.ArrayBoundsChecks, false)]
[Il2CppSetOption(Option.DivideByZeroChecks, false)]
public struct PhysicBodyComponent : IComponent
{
    public Rigidbody? Body;
}