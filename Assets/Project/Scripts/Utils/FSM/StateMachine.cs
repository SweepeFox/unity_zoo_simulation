using Cysharp.Threading.Tasks;

using System;
using System.Collections.Generic;

using UnityEngine;

namespace Utils.Fsm
{
    public sealed class StateMachine<TState, TEvent>
        where TState : struct, Enum
        where TEvent : struct, Enum
    {
        public readonly struct Transition
        {
            public Transition(TState source, TState destination, TEvent trigger)
            {
                this.Source = source;
                this.Destination = destination;
                this.Trigger = trigger;
            }
            public TState Source { get; }
            public TState Destination { get; }
            public TEvent Trigger { get; }
        }

        private sealed class Node
        {
            public Func<Transition, UniTask>? OnEntryAsync;
            public Func<Transition, UniTask>? OnExitAsync;
            public readonly Dictionary<TEvent, TState> Edges = new();
        }

        private readonly Dictionary<TState, Node> graph = new();
        private readonly Queue<(TEvent trigger, UniTaskCompletionSource<bool>? tcs)> queue = new();
        private bool processing;

        public TState State { get; private set; }

        public event Action<TState, TEvent>? UnhandledTrigger;
        public event Action<Transition>? Transitioned;

        public StateMachine(TState initial)
        {
            this.State = initial;
        }

        public void RegisterState(TState state, Func<Transition, UniTask>? onEntry, Func<Transition, UniTask>? onExit)
        {
            if (!this.graph.TryGetValue(state, out var node))
            {
                node = new Node();
                this.graph[state] = node;
            }
            node.OnEntryAsync = onEntry;
            node.OnExitAsync = onExit;
        }

        public void AddTransition(TState from, TEvent @event, TState to)
        {
            if (!this.graph.TryGetValue(from, out var node))
            {
                node = new Node();
                this.graph[from] = node;
            }
            node.Edges[@event] = to;
        }

        public bool IsTriggerValid(TEvent e) => this.graph.TryGetValue(this.State, out var node) && node.Edges.ContainsKey(e);

        public UniTask<bool> TriggerEventAsync(TEvent trigger)
        {
            var taskCompletionSource = new UniTaskCompletionSource<bool>();
            if (!this.IsTriggerValid(trigger))
            {
                UnhandledTrigger?.Invoke(this.State, trigger);
                taskCompletionSource.TrySetResult(false);
                return taskCompletionSource.Task;
            }

            this.queue.Enqueue((trigger, taskCompletionSource));
            if (!this.processing)
                _ = this.ProcessQueue();
            return taskCompletionSource.Task;
        }

        public void QueueTrigger(TEvent trigger)
        {
            if (!this.IsTriggerValid(trigger))
            {
                var permitted = this.graph.TryGetValue(this.State, out var n)
                    ? string.Join(", ", n.Edges.Keys)
                    : "";
                Debug.LogWarning($"[FSM] Cannot post '{trigger}' from '{this.State}'. Permitted: [{permitted}]");
                UnhandledTrigger?.Invoke(this.State, trigger);
                return;
            }

            this.queue.Enqueue((trigger, null));
            if (!this.processing)
                _ = this.ProcessQueue();
        }

        private async UniTask ProcessQueue()
        {
            this.processing = true;
            try
            {
                while (this.queue.Count > 0)
                {
                    var (trigger, triggerCompletionSource) = this.queue.Dequeue();
                    try
                    {
                        if (!this.IsTriggerValid(trigger))
                        {
                            UnhandledTrigger?.Invoke(this.State, trigger);
                            triggerCompletionSource?.TrySetResult(false);
                            continue;
                        }

                        var currentState = this.State;
                        var destinationState = this.graph[currentState].Edges[trigger];
                        var transition = new Transition(currentState, destinationState, trigger);

                        var currentGraphNode = this.graph[currentState];
                        if (currentGraphNode.OnExitAsync != null)
                            await currentGraphNode.OnExitAsync(transition);

                        this.State = destinationState;

                        var destinationNode = this.graph[destinationState];
                        if (destinationNode.OnEntryAsync != null)
                            await destinationNode.OnEntryAsync(transition);

                        Transitioned?.Invoke(transition);
                        triggerCompletionSource?.TrySetResult(true);
                    }
                    catch (Exception ex)
                    {
                        triggerCompletionSource?.TrySetException(ex);
                    }
                }
            }
            finally
            {
                this.processing = false;
            }
        }
    }
}
