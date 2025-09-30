using Constants;

using Scellecs.Morpeh;

using Services;

using Unity.IL2CPP.CompilerServices;

using UnityEngine;

[Il2CppSetOption(Option.NullChecks, false)]
[Il2CppSetOption(Option.ArrayBoundsChecks, false)]
[Il2CppSetOption(Option.DivideByZeroChecks, false)]
public sealed class SpawnSystem : ISystem
{
    public World World { get; set; } = null!;

    private readonly IPrefabProviderService prefabProviderService;

    private Stash<AnimalComponent> animalComponentStash = null!;
    private Stash<PhysicBodyComponent> physicBodyComponentStash = null!;

    private float spawnInterval;
    private float loopTime;

    public SpawnSystem(IPrefabProviderService prefabProviderService)
    {
        this.prefabProviderService = prefabProviderService;
    }

    public void OnAwake()
    {
        this.animalComponentStash = this.World.GetStash<AnimalComponent>();
        this.physicBodyComponentStash = this.World.GetStash<PhysicBodyComponent>();

        this.spawnInterval = Random.Range(1.0f, 2.0f);
    }

    public void OnUpdate(float deltaTime)
    {
        this.loopTime += deltaTime;

        if (this.loopTime > this.spawnInterval)
        {
            this.loopTime = 0.0f;
            this.spawnInterval = Random.Range(1.0f, 2.0f);

            this.SpawnAnimal();
        }
    }

    public void Dispose()
    {
    }

    private void SpawnAnimal()
    {
        var animalData = AnimalsData.GetRandomAnimalData();
        if (this.prefabProviderService.TryGet(animalData.PrefabAddress, out var animalPrefab))
        {
            var animalInstance = Object.Instantiate(animalPrefab, this.GetRandomPointOnGround(), Quaternion.identity);
            if (animalInstance == null)
            {
                Debug.LogError("Animal instance is null");
                return;
            }

            var animalEntity = this.World.CreateEntity();

            ref var animalComponent = ref this.animalComponentStash.Add(animalEntity);
            animalComponent.Type = animalData.Type;

            if (animalData.Type == AnimalType.PREDATOR)
            {
                ref var move = ref this.World.GetStash<LinearMoveComponent>().Add(animalEntity);
                move.Direction = new Vector3(Random.Range(-1f, 1f), 0f, Random.Range(-1f, 1f)).normalized;
                move.Speed = animalData.Speed;
            }
            else if (animalData.Type == AnimalType.PREY)
            {
                ref var move = ref this.World.GetStash<JumpMoveComponent>().Add(animalEntity);
                move.Direction = new Vector3(Random.Range(-1f, 1f), 0f, Random.Range(-1f, 1f)).normalized;
                move.JumpInterval = animalData.JumpInterval;
                move.JumpDistance = animalData.JumpDistance;
                move.Timer = move.JumpInterval;
            }

            ref var physicBodyComponent = ref this.physicBodyComponentStash.Add(animalEntity);
            physicBodyComponent.Body = animalInstance.GetComponent<Rigidbody>();

            if (animalInstance.TryGetComponent<CollisionReporter>(out var collisionReporter))
            {
                collisionReporter.Entity = animalEntity;
            }
        }
    }

    private Vector3 GetRandomPointOnGround()
    {
        var camHeight = Camera.main.transform.position.y;
        var viewportPos = new Vector3(Random.value, Random.value, camHeight);
        var worldPos = Camera.main.ViewportToWorldPoint(viewportPos);
        worldPos.y = 0f;
        return worldPos;
    }

}
