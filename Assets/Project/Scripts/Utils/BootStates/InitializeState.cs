using Cysharp.Threading.Tasks;

using Scellecs.Morpeh;

using System;

using UnityEngine;

using Utils.Fsm;

namespace BootStates
{
    public sealed class InitializeState : StateBase<BootState, BootEvent, BootContext>
    {
        protected override UniTask EnterCoreAsync(StateMachine<BootState, BootEvent> fsm, BootContext context)
        {
            this.InitEcs(context);

            fsm.QueueTrigger(BootEvent.Next);
            return UniTask.CompletedTask;
        }

        private void InitEcs(BootContext context)
        {
            Debug.Log("[Initialize] initializing ECS...");

            context.World = World.Default ?? World.Create();

            if (context.ServiceContainer == null)
            {
                throw new InvalidOperationException("Service provider is not initialized.");
            }

            var gameplaySystemsGroup = context.World.CreateSystemsGroup();
            gameplaySystemsGroup.AddSystem(context.ServiceContainer.CreateSystem<SpawnSystem>());
            gameplaySystemsGroup.AddSystem(context.ServiceContainer.CreateSystem<LinearMoveSystem>());
            gameplaySystemsGroup.AddSystem(context.ServiceContainer.CreateSystem<JumpMoveSystem>());
            gameplaySystemsGroup.AddSystem(context.ServiceContainer.CreateSystem<BordersSystem>());
            gameplaySystemsGroup.AddSystem(context.ServiceContainer.CreateSystem<CollisionSystem>());
            gameplaySystemsGroup.AddSystem(context.ServiceContainer.CreateSystem<DeadAnimalsCounterSystem>());
            gameplaySystemsGroup.AddSystem(context.ServiceContainer.CreateSystem<CleanupSystem>());

            context.World.AddSystemsGroup(1000, gameplaySystemsGroup);

            Debug.Log("[Initialize] ECS initialized.");
        }
    }
}
