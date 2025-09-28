using Cysharp.Threading.Tasks;

using Utils.Fsm;

namespace BootStates
{
    public sealed class StartState : StateBase<BootState, BootEvent, BootContext>
    {
        protected override UniTask EnterCoreAsync(StateMachine<BootState, BootEvent> fsm, BootContext context)
        {
            fsm.QueueTrigger(BootEvent.Next);
            return UniTask.CompletedTask;
        }
    }
}
