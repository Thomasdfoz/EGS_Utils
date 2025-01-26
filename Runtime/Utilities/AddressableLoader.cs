using EGS.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;


namespace EGS.Utils 
{
    public class AddressableLoader : MonoBehaviour
    {
        private static Dictionary<string, AsyncOperationHandle<UnityEngine.Object>> m_loadedAddressables = new Dictionary<string, AsyncOperationHandle<UnityEngine.Object>>();

        private static UnityEngine.Object m_processedAddressable;

        private static Dictionary<int, Action<UnityEngine.Object>> m_addressablesOnLoadingCallbacks = new Dictionary<int, Action<UnityEngine.Object>>();    

        private static int m_countOfAddressablesOnLoading = 0;

        public static IEnumerator GetAddressable(string key, Action<UnityEngine.Object> callback, Action<float> progress)
        {
            if (m_loadedAddressables.TryGetValue(key, out AsyncOperationHandle<UnityEngine.Object> value))
            {
                callback?.Invoke(value.Result);
                progress?.Invoke(1);
                yield break;
            }
            else
            {
                m_processedAddressable = null;
                progress?.Invoke(0);

                int operationIndex = ++m_countOfAddressablesOnLoading;
                m_addressablesOnLoadingCallbacks.Add(m_countOfAddressablesOnLoading, callback);

                Action<string, AsyncOperationHandle<UnityEngine.Object>> onRecieveAddresable = (key, operation) => OnRecieveAddressable(key, operation, operationIndex);

                yield return LoadAddressable(key, onRecieveAddresable, progress);
            }
        }

        public static void Release(string key)
        {
            if (m_loadedAddressables.TryGetValue(key, out AsyncOperationHandle<UnityEngine.Object> value))
            {
                Addressables.Release(value);
                Debugger.Log("Release Successful");

            }
            else
            {
                Debugger.Log("Key not found");
            }

        }

        public static void ReleaseAll()
        {
            foreach (var value in m_loadedAddressables.Values)
            {
                Addressables.Release(value);
                Debugger.Log($"Release {value} Successful");
            }
        }

        private static void OnRecieveAddressable(string key, AsyncOperationHandle<UnityEngine.Object> operation, int index)
        {
            if (m_loadedAddressables.ContainsKey(key))
                m_loadedAddressables.Add(key, operation);

            m_processedAddressable = operation.Result;

            ConsumeAddressableOnLoadingCallback(index);
            CheckResetOnLoadingCount();
        }

        private static void ConsumeAddressableOnLoadingCallback(int index)
        {
            m_addressablesOnLoadingCallbacks[index]?.Invoke(m_processedAddressable);
            m_addressablesOnLoadingCallbacks.Remove(index);
        }

        private static void CheckResetOnLoadingCount()
        {
            if (m_addressablesOnLoadingCallbacks.Count <= 0)
                m_countOfAddressablesOnLoading = 0;
        }

        private static IEnumerator LoadAddressable(string key, Action<string, AsyncOperationHandle<UnityEngine.Object>> callBack, Action<float> progress)
        {
            AsyncOperationHandle<UnityEngine.Object> operation = Addressables.LoadAssetAsync<UnityEngine.Object>(key);

            while (!operation.IsDone)
            {
                progress?.Invoke(operation.PercentComplete / 2);
                yield return null;
            }

            if (operation.Status == AsyncOperationStatus.Failed)
            {
                // Ocorreu um erro ao carregar o recurso
                Debug.LogWarning($"Erro ao carregar o recurso com a chave '{key}': {operation.OperationException}");

                yield break;

            }
            else
            {
                operation.Completed += op =>
                {
                    callBack?.Invoke(key, op);
                };
            }

            yield return new WaitUntil(() => operation.IsDone);

            progress?.Invoke(1);
        }
    }
}