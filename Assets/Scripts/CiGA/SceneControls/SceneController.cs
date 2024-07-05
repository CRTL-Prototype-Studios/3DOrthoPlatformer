using System.Collections;
using CiGA.Utility;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CiGA.SceneControls
{
    public class SceneController : Singleton<SceneController>
    {
        [SerializeField, Scene] private string transitionSceneName;
        private float _loadingProgress = 0f;
        public float LoadingProgress => _loadingProgress;

        public void LoadScene(string sceneName)
        {
            StartCoroutine(LoadSceneRoutine(sceneName));
        }

        private IEnumerator LoadSceneRoutine(string sceneName)
        {
            _loadingProgress = 0f;

            // Load the transition scene
            yield return SceneManager.LoadSceneAsync(transitionSceneName, LoadSceneMode.Additive);
            _loadingProgress = 0.1f;

            // Start loading the target scene in the background
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
            asyncLoad.allowSceneActivation = false;

            // Wait until the new scene is almost loaded
            while (!asyncLoad.isDone)
            {
                _loadingProgress = 0.1f + (asyncLoad.progress * 0.8f);
                yield return null;
            }

            // Activate the new scene
            asyncLoad.allowSceneActivation = true;
            yield return asyncLoad;

            _loadingProgress = 0.9f;

            // Unload the transition scene
            yield return SceneManager.UnloadSceneAsync(transitionSceneName);

            // Set the newly loaded scene as active
            Scene newScene = SceneManager.GetSceneByName(sceneName);
            SceneManager.SetActiveScene(newScene);

            // Unload the previous scene (if it's not the transition scene)
            Scene currentScene = SceneManager.GetActiveScene();
            if (currentScene.name != transitionSceneName && currentScene.name != sceneName)
            {
                yield return SceneManager.UnloadSceneAsync(currentScene);
            }

            _loadingProgress = 1f;
        }

        public float GetLoadingProgress()
        {
            return _loadingProgress;
        }
    }
}