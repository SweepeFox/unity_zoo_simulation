using Scellecs.Morpeh;

using TMPro;

using Unity.IL2CPP.CompilerServices;

[System.Serializable]
[Il2CppSetOption(Option.NullChecks, false)]
[Il2CppSetOption(Option.ArrayBoundsChecks, false)]
[Il2CppSetOption(Option.DivideByZeroChecks, false)]
public struct DeadAnimalsCounterComponent : IComponent
{
    public int DeadPreys;
    public int DeadPredators;
    public TextMeshProUGUI DeadPreysCounter;
    public TextMeshProUGUI DeadPredatorsCounter;
}