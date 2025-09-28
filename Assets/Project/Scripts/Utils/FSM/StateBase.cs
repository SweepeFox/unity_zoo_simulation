using Cysharp.Threading.Tasks;

using System;

using UnityEngine;

namespace Utils.Fsm
{
    public abstract class StateBase<TState, TEvent, TContext> : IState<TState, TEvent, TContext>
        where TContext : class
        where TState : struct, Enum
        where TEvent : struct, Enum
    {
        private readonly bool forceMainThread;

        protected StateBase(bool forceMainThread = true)
        {
            this.forceMainThread = forceMainThread;
        }

        public async UniTask OnEnterAsync(StateMachine<TState, TEvent> fsm, TContext context)
        {
            try
            {
                if (this.forceMainThread)
                    await UniTask.SwitchToMainThread();

                await this.EnterCoreAsync(fsm, context);
            }
            catch (Exception ex)
            {
                if (context is IErrorContext e)
                    e.Exception = ex;

                Debug.LogException(ex);
            }
        }

        public virtual UniTask OnExitAsync(StateMachine<TState, TEvent> fsm, TContext context)
            => UniTask.CompletedTask;

        protected virtual UniTask EnterCoreAsync(StateMachine<TState, TEvent> fsm, TContext context)
            => UniTask.CompletedTask;
    }
}
