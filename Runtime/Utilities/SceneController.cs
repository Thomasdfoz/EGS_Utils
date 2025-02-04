using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;


namespace EGS.Utils 
{
    public class SceneController : MonoBehaviour
    {
        public IEnumerator LoadSceneAsync(string sceneName)
        {
            AsyncOperation lOperation = SceneManager.LoadSceneAsync(sceneName);

            while (!lOperation.isDone)
                yield return null;

            Debug.Log($"Loading scene {sceneName}...");
        }
    }
}
