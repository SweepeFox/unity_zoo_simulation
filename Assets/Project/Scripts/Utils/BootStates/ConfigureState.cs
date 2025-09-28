using Cysharp.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;

using Services;

using UnityEngine;

using Utils.DI;
using Utils.Fsm;

namespace BootStates
{
    public sealed class ConfigureState : StateBase<BootState, BootEvent, BootContext>
    {
        protected override UniTask EnterCoreAsync(StateMachine<BootState, BootEvent> fsm, BootContext context)
        {
            var services = new ServiceCollection();

            services.AddSingleton<IPrefabProviderService, PrefabProviderService>();

            context.ServiceContainer = new ServiceContainer(services);
            Debug.Log("[Configure] container built");

            fsm.QueueTrigger(BootEvent.Next);
            return UniTask.CompletedTask;
        }
    }
}