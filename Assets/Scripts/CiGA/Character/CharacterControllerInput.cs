using System;
using UnityEngine;

namespace CiGA.Character
{
    public class CharacterControllerInput : MonoBehaviour
    {
        [SerializeField] protected CharacterControllerBehaviour Controller;
        [SerializeField] protected CharacterControlMap _map;

        protected void Awake()
        {
            if (!Controller) Controller = GetComponent<CharacterControllerBehaviour>();
            _map = new CharacterControlMap();
        }

        protected void OnEnable()
        {
            _map.Enable();
            
        }

        protected void OnDisable()
        {
            _map.Disable();
            
        }
    }
}