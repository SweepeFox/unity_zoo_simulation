using BootStates;

using Cysharp.Threading.Tasks;

using Scellecs.Morpeh;

using System;
using System.Collections.Generic;
using System.Threading;

using UnityEngine;

using Utils.DI;
using Utils.Fsm;

public enum BootState
{
    Start,
    Configure,
    LoadAssets,
    WaitingState,
    Initialize,
    Gameplay,
    Error
}

public enum BootEvent
{
    Next,
    Fail,
    Restart
}

public sealed class BootContext : ICancellableContext, IErrorContext
{
    public World? World { get; set; }
    public ServiceContainer? ServiceContainer { get; set; }
    public float Progress { get; set; } = 0f;
    public Exception? Exception { get; set; }
    public CancellationToken Token => this.Cancellation.Token;
    public CancellationTokenSource Cancellation { get; } = new();
    public List<Action> UnsubscribeAll { get; } = new();
    public UniTask? LoadingTask { get; set; } = null;
}

public sealed class Boot : MonoBehaviour
{
    private FsmRunner<BootState, BootEvent, BootContext>? runner;
    private readonly BootContext context = new();

    private async void Start()
    {
        var states = new Dictionary<BootState, IState<BootState, BootEvent, BootContext>>
        {
            [BootState.Start] = new StartState(),
            [BootState.Configure] = new ConfigureState(),
            [BootState.LoadAssets] = new LoadAssetsState(),
            [BootState.WaitingState] = new WaitingState(),
            [BootState.Initialize] = new InitializeState(),
            [BootState.Gameplay] = new GameplayState(),
            [BootState.Error] = new ErrorState(),
        };

        var transitions = new Dictionary<BootState, Dictionary<BootEvent, BootState>>
        {
            [BootState.Start] = new() { [BootEvent.Next] = BootState.Configure },
            [BootState.Configure] = new() { [BootEvent.Next] = BootState.LoadAssets, [BootEvent.Fail] = BootState.Error },
            [BootState.LoadAssets] = new() { [BootEvent.Next] = BootState.WaitingState, [BootEvent.Fail] = BootState.Error },
            [BootState.WaitingState] = new() { [BootEvent.Next] = BootState.Initialize, [BootEvent.Fail] = BootState.Error },
            [BootState.Initialize] = new() { [BootEvent.Next] = BootState.Gameplay, [BootEvent.Fail] = BootState.Error },
            [BootState.Error] = new() { [BootEvent.Restart] = BootState.Start },
        };

        this.runner = new FsmBuilder<BootState, BootEvent, BootContext>(BootState.Start)
            .WithStates(states)
            .WithTransitions(transitions)
            .WithFinalStates(BootState.Gameplay)
            .OnUnhandledTrigger((s, e) => Debug.LogWarning($"BootFsm: Unhandled trigger: {s} ← {e}"))
            .OnTransitionedTrigger((t, ctx) => Debug.Log($"BootFsm: {t.Source} --({t.Trigger})-> {t.Destination}"))
            .Build(this.context);

        await this.runner.StartAsync();
    }

    private void OnDestroy()
    {
        this.context.Cancellation.Cancel();
        this.context.Cancellation.Dispose();
        this.context.World?.Dispose();

        foreach (var unsubscribe in this.context.UnsubscribeAll)
        {
            unsubscribe();
        }
    }
}