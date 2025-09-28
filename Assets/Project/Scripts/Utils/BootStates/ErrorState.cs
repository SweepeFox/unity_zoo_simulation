using Cysharp.Threading.Tasks;

using UnityEngine;

using Utils.Fsm;

namespace BootStates
{
    public sealed class ErrorState : StateBase<BootState, BootEvent, BootContext>
    {
        protected override UniTask EnterCoreAsync(StateMachine<BootState, BootEvent> fsm, BootContext context)
        {
            if (context.Exception != null)
            {
                Debug.LogError(context.Exception.ToString());
            }

            return UniTask.CompletedTask;
        }
    }
}