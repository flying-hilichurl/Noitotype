
using System;
using System.Threading.Tasks;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Utility
{
    public static class AddressableUtility
    {
        public static async Task<T> LoadAssetByNameAsync<T>(string name)
        {
            AsyncOperationHandle<T> handle = Addressables.LoadAssetAsync<T>(name);
            await handle.Task;
            
            if(handle.Status == AsyncOperationStatus.Succeeded)
                return handle.Result;
            else
                throw new InvalidOperationException("未获取到资源");
        }
    }
}