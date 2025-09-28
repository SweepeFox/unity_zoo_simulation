using Cysharp.Threading.Tasks;

using System.Collections.Generic;

using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Services
{
    public sealed class PrefabProviderService : IPrefabProviderService
    {
        private readonly Dictionary<string, AsyncOperationHandle<GameObject>> handles = new();

        public async UniTask<GameObject> LoadAsync(string address)
        {
            if (this.handles.TryGetValue(address, out var h))
            {
                return h.Result;
            }

            var handle = Addressables.LoadAssetAsync<GameObject>(address);

            await handle.Task;
            this.handles[address] = handle;

            Debug.Log($"[PrefabProvider] loaded '{address}'");

            return handle.Result;
        }

        public bool TryGet(string address, out GameObject? prefab)
        {
            if (!this.handles.TryGetValue(address, out var h))
            {
                prefab = null;
                return false;
            }
            if (!h.IsDone)
            {
                prefab = null;
                return false;
            }
            prefab = h.Result;
            return prefab != null;
        }

        public void Release(string address)
        {
            if (!this.handles.TryGetValue(address, out var h))
            {
                return;
            }
            Addressables.Release(h);
            this.handles.Remove(address);
        }

        public void ReleaseAll()
        {
            foreach (var kv in this.handles)
            {
                Addressables.Release(kv.Value);
            }
            this.handles.Clear();
        }
    }
}
