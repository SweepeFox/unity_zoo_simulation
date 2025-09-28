using Cysharp.Threading.Tasks;

using Utils.Fsm;

namespace BootStates
{
    public sealed class WaitingState : StateBase<BootState, BootEvent, BootContext>
    {
        protected override UniTask EnterCoreAsync(StateMachine<BootState, BootEvent> fsm, BootContext context)
        {
            if (context.LoadingTask == null)
            {
                fsm.QueueTrigger(BootEvent.Next);

                return UniTask.CompletedTask;
            }

            context.LoadingTask.Value.ContinueWith(() =>
            {
                fsm.QueueTrigger(BootEvent.Next);
            })
            .Forget((exception) =>
            {
                fsm.QueueTrigger(BootEvent.Fail);
                context.LoadingTask = null;
                context.Exception = exception;
            });

            return UniTask.CompletedTask;
        }
    }
}
