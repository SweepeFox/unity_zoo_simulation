using Constants;
using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;
using UnityEngine;
using System.Collections.Generic;

[Il2CppSetOption(Option.NullChecks, false)]
[Il2CppSetOption(Option.ArrayBoundsChecks, false)]
[Il2CppSetOption(Option.DivideByZeroChecks, false)]
public sealed class CollisionSystem : ISystem
{
    public World World { get; set; } = null!;

    private Filter filter = null!;
    private Stash<CollisionEventComponent> eventStash = null!;
    private Stash<AnimalComponent> animalStash = null!;
    private Stash<PhysicBodyComponent> bodyStash = null!;
    public Stash<DeadComponent> deadStash = null!;

    private Dictionary<(AnimalType, AnimalType), CollisionHandler> interactions = null!;

    public void OnAwake()
    {
        this.filter = this.World.Filter.With<CollisionEventComponent>().Build();
        this.eventStash = this.World.GetStash<CollisionEventComponent>();
        this.animalStash = this.World.GetStash<AnimalComponent>();
        this.bodyStash = this.World.GetStash<PhysicBodyComponent>();
        this.deadStash = this.World.GetStash<DeadComponent>();

        this.interactions = new Dictionary<(AnimalType, AnimalType), CollisionHandler> {
            { (AnimalType.PREY, AnimalType.PREY), (self, other, sys) =>
            {
                return;
            }},
            { (AnimalType.PREDATOR, AnimalType.PREY), (self, other, sys) =>
            {
                sys.deadStash.Add(other);
                sys.ShowTasty(self);
            }},
            { (AnimalType.PREY, AnimalType.PREDATOR), (self, other, sys) =>
            {
                sys.deadStash.Add(self);
                sys.ShowTasty(other);
            }},
            { (AnimalType.PREDATOR, AnimalType.PREDATOR), (self, other, sys) =>
            {
                if (Random.value < 0.5f)
                {
                    sys.deadStash.Add(self);
                    sys.ShowTasty(other);
                }
                else {
                    sys.deadStash.Add(other);
                    sys.ShowTasty(self);
                }
            }},
        };
    }

    public void OnUpdate(float deltaTime)
    {
        foreach (var evtEntity in this.filter)
        {
            ref var evt = ref this.eventStash.Get(evtEntity);

            if (this.World.IsDisposed(evt.Self) || this.World.IsDisposed(evt.Other))
            {
                continue;
            }

            if (!this.animalStash.Has(evt.Self) || !this.animalStash.Has(evt.Other))
            {
                continue;
            }

            ref var a = ref this.animalStash.Get(evt.Self);
            ref var b = ref this.animalStash.Get(evt.Other);

            var key = (a.Type, b.Type);
            if (this.interactions.TryGetValue(key, out var handler))
            {
                handler(evt.Self, evt.Other, this);
            }

            this.World.RemoveEntity(evtEntity);
        }
    }

    public void Dispose() { }

    private void ShowTasty(Entity predator)
    {
        if (!this.bodyStash.Has(predator))
            return;
        ref var body = ref this.bodyStash.Get(predator);
        if (body.Body == null)
            return;

        var go = new GameObject("TastyLabel");
        var tm = go.AddComponent<TextMesh>();
        tm.text = "Tasty!";
        tm.fontSize = 32;
        tm.color = Color.yellow;
        go.transform.SetParent(body.Body.transform);
        go.transform.localPosition = new Vector3(0, -1f, 0);
        Object.Destroy(go, 1.5f);
    }
}

public delegate void CollisionHandler(Entity self, Entity other, CollisionSystem system);