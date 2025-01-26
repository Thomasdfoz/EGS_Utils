using EGS.Utils;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;


namespace EGS.Utils 
{
    public class SceneController : SingletonBehaviour<SceneController>
    {
        public IEnumerator LoadSceneAsync(string sceneName)
        {
            AsyncOperation lOperation = SceneManager.LoadSceneAsync(sceneName);

            while (!lOperation.isDone)
                yield return null;

            Debugger.Log($"Loading scene {sceneName}...");
        }
    }
}
