using Cysharp.Threading.Tasks;

using Utils.Fsm;

namespace BootStates
{
    public sealed class GameplayState : StateBase<BootState, BootEvent, BootContext>
    {
        protected override UniTask EnterCoreAsync(StateMachine<BootState, BootEvent> fsm, BootContext context)
        {
            return UniTask.CompletedTask;
        }
    }
}