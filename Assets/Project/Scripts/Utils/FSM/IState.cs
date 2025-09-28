using Cysharp.Threading.Tasks;

using System;

namespace Utils.Fsm
{
    public interface IState<TState, TEvent, TContext>
        where TContext : class
        where TState : struct, Enum
        where TEvent : struct, Enum
    {
        UniTask OnEnterAsync(StateMachine<TState, TEvent> fsm, TContext context);
        UniTask OnExitAsync(StateMachine<TState, TEvent> fsm, TContext context);
    }
}
