using Constants;

using Cysharp.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;

using Services;

using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

using Utils.Fsm;

namespace BootStates
{
    public sealed class LoadAssetsState : StateBase<BootState, BootEvent, BootContext>
    {
        protected override UniTask EnterCoreAsync(StateMachine<BootState, BootEvent> fsm, BootContext context)
        {
            Debug.Log("[LoadAssets] initializing...");
            this.LogProgress(context, 0f, 0f, 0f);

            var tasks = new Func<BootContext, float, float, UniTask>[]
            {
                this.LoadPrefabs,
                this.DownloadAssets
            };

            var total = tasks.Length;
            var taskList = new UniTask[total];
            for (int i = 0; i < total; i++)
            {
                var (from, to) = this.GetProgressRange(i, total);
                taskList[i] = tasks[i](context, from, to);
            }

            context.LoadingTask = UniTask.WhenAll(taskList)
                .AttachExternalCancellation(context.Token)
                .ContinueWith(() =>
                {
                    this.LogProgress(context, 1f, 1f, 1f);
                    Debug.Log("[LoadAssets] all assets loaded.");

                    context.LoadingTask = null;
                });

            fsm.QueueTrigger(BootEvent.Next);
            return UniTask.CompletedTask;
        }

        private void LogProgress(BootContext context, float from, float to, float percent)
        {
            context.Progress = from + ((to - from) * percent);
            Debug.Log($"[LoadAssets] progress: {context.Progress:P}");
        }

        private (float from, float to) GetProgressRange(int index, int total)
        {
            float from = (float)index / total;
            float to = (float)(index + 1) / total;
            return (from, to);
        }

        private async UniTask LoadPrefabs(BootContext context, float from, float to)
        {
            if (context.ServiceContainer == null)
                throw new InvalidOperationException("Service provider is not initialized.");

            var prefabProvider = context.ServiceContainer.ServiceProvider.GetService<IPrefabProviderService>()
                ?? throw new InvalidOperationException("Prefab provider service is not registered.");

            var addresses = new List<string>();

            foreach (var animalData in AnimalsData.Animals)
            {
                addresses.Add(animalData.PrefabAddress);
            }

            var total = addresses.Count;
            for (int i = 0; i < total; i++)
            {
                if (context.Token.IsCancellationRequested)
                {
                    Debug.LogWarning("[LoadViewAssets] load view assets cancelled by token");
                    return;
                }

                var (addrFrom, addrTo) = this.GetProgressRange(i, total);
                var (rangeFrom, rangeTo) = (from + ((to - from) * addrFrom), from + ((to - from) * addrTo));

                await prefabProvider.LoadAsync(addresses[i]);
                this.LogProgress(context, rangeFrom, rangeTo, 1f);
            }

            this.LogProgress(context, from, to, to);
            Debug.Log("[LoadAssets] view assets loaded.");
        }

        private async UniTask DownloadAssets(BootContext context, float from, float to)
        {
            Debug.Log("[LoadAssets] loading addressables...");
            await Addressables.InitializeAsync();

            var sizeH = Addressables.GetDownloadSizeAsync(Constants.AddressLabels.PRELOAD);
            await sizeH;

            if (sizeH.Result > 0)
            {
                var dl = Addressables.DownloadDependenciesAsync(Constants.AddressLabels.PRELOAD, false);
                try
                {
                    while (!dl.IsDone)
                    {
                        if (context.Token.IsCancellationRequested)
                        {
                            Debug.LogWarning("[DownloadAssets] download addressables cancelled by token");
                            Addressables.Release(dl);
                            return;
                        }

                        this.LogProgress(context, from, to, dl.GetDownloadStatus().Percent);

                        await UniTask.Yield();
                    }
                    await dl.Task;

                    if (!dl.IsValid() || dl.Status != AsyncOperationStatus.Succeeded)
                    {
                        Debug.LogError($"[LoadAssets] Download failed. Status={dl.Status}. Error={dl.OperationException}");
                    }
                }
                finally
                {
                    Addressables.Release(dl);
                }
            }
            else
            {
                Debug.Log("[LoadAssets] no assets to download");
            }

            this.LogProgress(context, from, to, to);
            Debug.Log("[LoadAssets] addressables loaded.");
        }
    }
}
