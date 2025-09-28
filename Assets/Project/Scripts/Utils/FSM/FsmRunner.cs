using Cysharp.Threading.Tasks;

using System;
using System.Collections.Generic;

namespace Utils.Fsm
{
    public sealed class FsmRunner<TState, TEvent, TContext>
        where TContext : class
        where TState : struct, Enum
        where TEvent : struct, Enum
    {
        private readonly IDictionary<TState, IState<TState, TEvent, TContext>> states;
        private readonly HashSet<TState> finalStates;
        private readonly UniTaskCompletionSource<TState> completedTcs = new();

        public StateMachine<TState, TEvent> Fsm { get; }
        public TContext Context { get; }
        public UniTask<TState> Completed => this.completedTcs.Task;

        internal FsmRunner(
            StateMachine<TState, TEvent> fsm,
            IDictionary<TState, IState<TState, TEvent, TContext>> states,
            HashSet<TState> finalStates,
            TContext context)
        {
            this.Fsm = fsm;
            this.states = states;
            this.finalStates = finalStates;
            this.Context = context;
        }

        public UniTask StartAsync()
        {
            return this.states[this.Fsm.State].OnEnterAsync(this.Fsm, this.Context);
        }

        internal void RaiseOnTransitioned(StateMachine<TState, TEvent>.Transition t)
        {
            if (this.finalStates.Contains(t.Destination))
                this.completedTcs.TrySetResult(t.Destination);
        }
    }
}
