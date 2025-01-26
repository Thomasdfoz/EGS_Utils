using EGS.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;


namespace EGS.Utils 
{
    public class AddressableHelper
    {
        public static IEnumerator LoadAllAssetsFromBundle(string bundleUrl, Action<UnityEngine.Object> callback)
        {
            using (UnityWebRequest uwr = UnityWebRequestAssetBundle.GetAssetBundle(bundleUrl))
            {
                yield return uwr.SendWebRequest();

                if (uwr.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError("Failed to download AssetBundle: " + uwr.error);
                    yield break;
                }

                AssetBundle bundle = DownloadHandlerAssetBundle.GetContent(uwr);

                UnityEngine.Object[] prefab = bundle.LoadAllAssets();

                for (int i = 0; i < prefab.Length; i++)
                {
                    callback?.Invoke(prefab[i]);
                }

                bundle.Unload(false);
            }
        }
        public static IEnumerator LoadAssetFromBundle(string bundleUrl, string assetname, Action<UnityEngine.Object> callback, Type type = null)
        {
            using (UnityWebRequest uwr = UnityWebRequestAssetBundle.GetAssetBundle(bundleUrl))
            {
                yield return uwr.SendWebRequest();

                if (uwr.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError("Failed to download AssetBundle: " + uwr.error);
                    yield break;
                }

                AssetBundle bundle = DownloadHandlerAssetBundle.GetContent(uwr);

                UnityEngine.Object[] prefab = bundle.LoadAllAssets();

                for (int i = 0; i < prefab.Length; i++)
                {
                    if (prefab[i].name == assetname)
                    {
                        if (type != null)
                        {
                            if (prefab[i].GetType() == type)
                            {
                                callback?.Invoke(prefab[i]);
                                yield break;
                            }
                        }
                        else
                        {
                            callback?.Invoke(prefab[i]);
                            yield break;
                        }
                    }
                }

                bundle.Unload(false);
            }
        }

        public static IEnumerator LoadCatalog(string pathOrUrl, string folderName = null, string catalogName = null,
            Action<float> progress = null)
        {
            progress?.Invoke(0);

            yield return Addressables.InitializeAsync();

            progress?.Invoke(50);

            yield return Addressables.LoadContentCatalogAsync(pathOrUrl + folderName + catalogName, true);

            progress?.Invoke(100);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"> AssetReference, string name</param> 
        /// <param name="callBack"></param>
        /// <param name="progress"></param>
        /// <returns></returns>
        public static IEnumerator InstantiateAddressable(object key, Action<GameObject> callBack,
            Action<float> progress)
        {
            AsyncOperationHandle<GameObject> operation = Addressables.InstantiateAsync(key);

            while (!operation.IsDone)
            {
                progress?.Invoke((operation.PercentComplete / 2));
                yield return null;
            }

            operation.Completed += op =>
            {
                if (op.Status == AsyncOperationStatus.Succeeded)
                {
                    callBack(op.Result);
                }
                else if (op.Status == AsyncOperationStatus.Failed)
                {
                    Debugger.LogError(string.Format($"Fail to load asset: {key}"));
                }
            };

            yield return new WaitUntil(() => operation.IsDone);

            progress?.Invoke(100);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="label">all addressables of this label will be instantiated</param>
        /// <param name="callbackOnEachDownloadFinished"></param>
        /// <param name="callbackAllDownloadsFinished"></param>
        /// <param name="progress"></param>
        /// <returns></returns>
        public static IEnumerator InstantiateAddressables(string label,
            Action<UnityEngine.Object> callbackOnEachDownloadFinished, Action callbackAllDownloadsFinished,
            Action<float> progress)
        {
            AsyncOperationHandle<IList<IResourceLocation>> operation = Addressables.LoadResourceLocationsAsync(label);

            while (!operation.IsDone)
            {
                progress?.Invoke(operation.PercentComplete);
                yield return null;
            }

            if (operation.Status == AsyncOperationStatus.Succeeded)
            {
                int totalAssets = operation.Result.Count;
                int currentAssets = 0;
                float eachProgress = 1f / totalAssets;
                for (int i = 0; i < totalAssets; i++)
                {
                    yield return InstantiateAddressable(operation.Result[i].PrimaryKey, callbackOnEachDownloadFinished, progress);

                    currentAssets++;
                    if (currentAssets >= totalAssets) callbackAllDownloadsFinished?.Invoke();
                }
            }
            else if (operation.Status == AsyncOperationStatus.Failed)
            {
                Debugger.LogError(string.Format("Fail to load assets with label: {0}", label));
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"> AssetReference, string name</param> 
        /// <param name="callBack"></param>
        /// <param name="progress"></param>
        /// <returns></returns>
        public static IEnumerator LoadAddressable(object key, Action<UnityEngine.Object> callBack,
            Action<float> progress)
        {
            AsyncOperationHandle<UnityEngine.Object> operation = Addressables.LoadAssetAsync<UnityEngine.Object>(key);

            while (!operation.IsDone)
            {
                progress?.Invoke((operation.PercentComplete / 2));
                yield return null;
            }

            operation.Completed += op =>
            {
                if (op.Status == AsyncOperationStatus.Succeeded)
                {
                    callBack(op.Result);
                }
                else if (op.Status == AsyncOperationStatus.Failed)
                {
                    Debugger.LogError(string.Format($"Fail to load asset: {key}"));
                }
            };

            yield return new WaitUntil(() => operation.IsDone);

            Addressables.Release(operation);

            progress?.Invoke(100);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="label">all addressables of this label will be loaded</param>
        /// <param name="callbackOnEachDownloadFinished"></param>
        /// <param name="callbackAllDownloadsFinished"></param>
        /// <param name="progress"></param>
        /// <returns></returns>
        public static IEnumerator LoadAddressables(string label,
            Action<UnityEngine.Object> callbackOnEachDownloadFinished, Action callbackAllDownloadsFinished,
            Action<float> progress)
        {
            AsyncOperationHandle<IList<IResourceLocation>> operation = Addressables.LoadResourceLocationsAsync(label);

            while (!operation.IsDone)
            {
                progress?.Invoke(operation.PercentComplete);
                yield return null;
            }

            if (operation.Status == AsyncOperationStatus.Succeeded)
            {
                int totalAssets = operation.Result.Count;
                int currentAssets = 0;
                float eachProgress = 1f / totalAssets;
                for (int i = 0; i < totalAssets; i++)
                {
                    yield return InstantiateAddressable(operation.Result[i].PrimaryKey, callbackOnEachDownloadFinished, progress);

                    currentAssets++;
                    if (currentAssets >= totalAssets) callbackAllDownloadsFinished?.Invoke();
                }
            }
            else if (operation.Status == AsyncOperationStatus.Failed)
            {
                Debugger.LogError(string.Format("Fail to load assets with label: {0}", label));
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"> AssetReference, string name</param>
        /// <returns></returns>
        public static GameObject InstantiateAddressableSync(object key)
        {
            AsyncOperationHandle<GameObject> operation = Addressables.InstantiateAsync(key);
            operation.WaitForCompletion();

            if (operation.Status != AsyncOperationStatus.Succeeded)
            {
                Debugger.LogError(string.Format($"Fail to load asset: {key}"));
                return null;
            }

            return operation.Result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="label">all addressables of this label will be instantiated</param>
        /// <returns></returns>
        public static IEnumerable<GameObject> InstantiateAddressablesSync(string label)
        {
            AsyncOperationHandle<IList<IResourceLocation>> operation = Addressables.LoadResourceLocationsAsync(label);
            operation.WaitForCompletion();

            if (operation.Status != AsyncOperationStatus.Succeeded)
            {
                Debugger.LogError(string.Format("Fail to load assets with label: {0}", label));
                return null;
            }

            var instantiatedObjects = new List<GameObject>();
            int totalAssets = operation.Result.Count;
            for (int i = 0; i < totalAssets; i++)
            {
                var obj = InstantiateAddressableSync(operation.Result[i]);

                if (obj == null) return null;

                instantiatedObjects.Add(obj);
            }

            return instantiatedObjects;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"> AssetReference, string name</param>
        /// <returns></returns>
        public static UnityEngine.Object LoadAddressableSync(object key)
        {
            AsyncOperationHandle<UnityEngine.Object> operation = Addressables.LoadAssetAsync<UnityEngine.Object>(key);
            operation.WaitForCompletion();

            if (operation.Status != AsyncOperationStatus.Succeeded)
            {
                Debugger.LogError(string.Format($"Fail to load asset: {key}"));
                return null;
            }

            return operation.Result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="label">all addressables of this label will be loaded</param>
        /// <returns></returns>
        public static IEnumerable<UnityEngine.Object> LoadAddressablesSync(string label)
        {
            AsyncOperationHandle<IList<IResourceLocation>> operation = Addressables.LoadResourceLocationsAsync(label);
            operation.WaitForCompletion();

            if (operation.Status != AsyncOperationStatus.Succeeded)
            {
                Debugger.LogError(string.Format("Fail to load assets with label: {0}", label));
            }

            var instantiatedObjects = new List<GameObject>();

            int totalAssets = operation.Result.Count;
            for (int i = 0; i < totalAssets; i++)
            {
                var obj = InstantiateAddressableSync(operation.Result[i]);

                if (obj == null) return null;

                instantiatedObjects.Add(obj);
            }

            return instantiatedObjects;
        }


        /// <summary>
        /// used this only to loadScene for addressable
        /// </summary>
        /// <param name="sceneName"></param>
        /// <param name="loadingMode"></param>
        /// <param name="OnCompletedDownload"></param>
        /// <param name="onProgress"></param>
        /// <param name="tryIndex"></param>
        /// <returns></returns>
        public static IEnumerator RequestAndDownloadAndLoadScene(string sceneName, LoadSceneMode loadingMode,
            Action<SceneInstance> OnCompletedDownload, Action<float> onProgress, int tryIndex = 0)
        {
            var sizeAsync = Addressables.GetDownloadSizeAsync(sceneName);

            while (!sizeAsync.IsDone || sizeAsync.Status == AsyncOperationStatus.Failed)
                yield return null;

            if (sizeAsync.Status == AsyncOperationStatus.Failed)
            {
                Debugger.LogError(string.Format("Fail to load Scene with name: {0}", sceneName));
                yield break;
            }

            Debugger.Log("Initializing downloading scene (" + sceneName + ") Size: " +
                         sizeAsync.Result.ToString("0.000"));

            var operation = Addressables.LoadSceneAsync(sceneName, loadingMode, true);
            Debugger.Log("Downloading scene async (" + sceneName + ")");

            while (!operation.IsDone)
            {
                Debugger.Log("Progress downloading scene (" + sceneName + ") in " + operation.PercentComplete);
                onProgress?.Invoke(operation.PercentComplete);
                yield return null;
            }

            if (operation.Status == AsyncOperationStatus.Succeeded)
            {
                OnCompletedDownload?.Invoke(operation.Result);
            }
            else if (operation.Status == AsyncOperationStatus.Failed)
            {
                Debugger.LogError(string.Format("Fail to load Scene with name: {0}", sceneName));
            }
        }

        public static void UnloadScene(AsyncOperationHandle operation)
        {
            Addressables.UnloadSceneAsync(operation, true).Completed += op =>
            {
                switch (op.Status)
                {
                    case AsyncOperationStatus.Succeeded:
                        Debugger.Log("Level Scene successfully unloaded");
                        break;
                    case AsyncOperationStatus.Failed:
                        Debugger.Log("There was an error unloading Level Scene");
                        break;
                    default:
                        break;
                }
            };
        }

        public static void ReleaseHandle(AsyncOperationHandle operation)
        {
            try
            {
                Addressables.Release(operation);
                Debugger.Log("Handle released successfully!");
            }
            catch (Exception)
            {
                Debugger.LogError("Not possible to release the level handle...");
            }
        }
    }
}