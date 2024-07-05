using System;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace CiGAGJ2024.SceneControls
{
    public class MainMenuScene : MonoBehaviour
    {
        [SerializeField] private Button start;
        [SerializeField, Scene] private string nextScene;

        private void Awake()
        {
            start.onClick.AddListener(() =>
            {
                SceneController.Instance.LoadScene(nextScene);
            });
        }
    }
}
