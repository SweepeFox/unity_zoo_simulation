using System;
using System.Collections.Generic;

namespace Utils.Fsm
{
    public sealed class FsmBuilder<TState, TEvent, TContext>
        where TContext : class
        where TState : struct, Enum
        where TEvent : struct, Enum
    {
        private readonly TState initialState;
        private readonly HashSet<TState> finalStates = new();

        private IDictionary<TState, IState<TState, TEvent, TContext>> states = new Dictionary<TState, IState<TState, TEvent, TContext>>();
        private IDictionary<TState, Dictionary<TEvent, TState>> transitions = new Dictionary<TState, Dictionary<TEvent, TState>>();

        private Action<TState, TEvent>? onUnhandled;
        private Action<StateMachine<TState, TEvent>.Transition, TContext>? onTransitioned;

        public FsmBuilder(TState initialState)
        {
            this.initialState = initialState;
        }

        public FsmBuilder<TState, TEvent, TContext> WithStates(IDictionary<TState, IState<TState, TEvent, TContext>> value)
        {
            this.states = value;
            return this;
        }

        public FsmBuilder<TState, TEvent, TContext> WithTransitions(IDictionary<TState, Dictionary<TEvent, TState>> value)
        {
            this.transitions = value;
            return this;
        }

        public FsmBuilder<TState, TEvent, TContext> AddTransition(TState from, TEvent @event, TState to)
        {
            if (!this.transitions.TryGetValue(from, out var map))
            {
                map = new();
                this.transitions[from] = map;
            }
            map[@event] = to;
            return this;
        }

        public FsmBuilder<TState, TEvent, TContext> WithFinalStates(params TState[] states)
        {
            foreach (var s in states)
                this.finalStates.Add(s);
            return this;
        }

        public FsmBuilder<TState, TEvent, TContext> OnUnhandledTrigger(Action<TState, TEvent> handler)
        {
            this.onUnhandled = handler;
            return this;
        }

        public FsmBuilder<TState, TEvent, TContext> OnTransitionedTrigger(Action<StateMachine<TState, TEvent>.Transition, TContext> handler)
        {
            this.onTransitioned = handler;
            return this;
        }

        public FsmRunner<TState, TEvent, TContext> Build(TContext context)
        {
            foreach (var s in this.states.Keys)
            {
                if (this.finalStates.Contains(s))
                    continue;
                if (!this.transitions.TryGetValue(s, out var map) || map.Count == 0)
                    throw new InvalidOperationException($"State {s} has no outgoing transitions");

                foreach (var to in map.Values)
                {
                    if (!this.states.ContainsKey(to))
                        throw new InvalidOperationException($"Transition {s} -> {to} references missing state");
                }
            }
            foreach (var s in this.finalStates)
            {
                if (this.transitions.TryGetValue(s, out var map) && map.Count > 0)
                    throw new InvalidOperationException($"Final state {s} has outgoing transitions");
            }

            var fsm = new StateMachine<TState, TEvent>(this.initialState);

            foreach (var kv in this.states)
            {
                var state = kv.Key;
                var st = kv.Value;
                fsm.RegisterState(
                    state,
                    tr => st.OnEnterAsync(fsm, context),
                    tr => st.OnExitAsync(fsm, context)
                );
            }

            foreach (var kv in this.transitions)
            {
                var from = kv.Key;
                foreach (var tr in kv.Value)
                    fsm.AddTransition(from, tr.Key, tr.Value);
            }

            if (this.onUnhandled != null)
                fsm.UnhandledTrigger += (s, e) => this.onUnhandled?.Invoke(s, e);

            var runner = new FsmRunner<TState, TEvent, TContext>(fsm, this.states, this.finalStates, context);

            fsm.Transitioned += tr =>
            {
                this.onTransitioned?.Invoke(tr, context);
                runner.RaiseOnTransitioned(tr);
            };

            return runner;
        }
    }
}
