using NaughtyAttributes;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace CiGA.SceneControls
{
    public class TransitionScene : MonoBehaviour
    {
        [SerializeField] private Slider slider;
        [SerializeField, Scene] private string transitionScene;
        private bool _allowUpdateStatus = false;
    
        void Start()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
        {
            if (scene.name.Equals(transitionScene))
                _allowUpdateStatus = true;
        }

        // Update is called once per frame
        void FixedUpdate()
        {
            if (_allowUpdateStatus)
                slider.value = SceneController.Instance.LoadingProgress;
            if (SceneController.Instance.LoadingProgress >= 1f) _allowUpdateStatus = false;
        }
    }
}
