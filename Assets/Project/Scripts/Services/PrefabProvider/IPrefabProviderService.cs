using Cysharp.Threading.Tasks;

using UnityEngine;

namespace Services
{
    public interface IPrefabProviderService
    {
        UniTask<GameObject> LoadAsync(string address);
        bool TryGet(string address, out GameObject? prefab);
        void Release(string address);
        void ReleaseAll();
    }
}