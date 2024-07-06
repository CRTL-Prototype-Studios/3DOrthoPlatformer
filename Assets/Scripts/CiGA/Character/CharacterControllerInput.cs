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
            _map.Primary.Movement.performed += Controller.ListenMovementPerformed;
            _map.Primary.Movement.canceled += Controller.ListenMovementCancelled;
            _map.Primary.Jump.performed += Controller.ListenJump;
            _map.Primary.Push.performed += Controller.ListenPushPerformed;
            _map.Primary.Push.canceled += Controller.ListenPushCanceled;
            _map.Primary.Direction.performed += Controller.ListenDirectionPerformed;
        }

        protected void OnDisable()
        {
            _map.Disable();
            _map.Primary.Movement.performed -= Controller.ListenMovementPerformed;
            _map.Primary.Movement.canceled -= Controller.ListenMovementCancelled;
            _map.Primary.Jump.performed -= Controller.ListenJump;
            _map.Primary.Push.performed -= Controller.ListenPushPerformed;
            _map.Primary.Push.canceled -= Controller.ListenPushCanceled;
            _map.Primary.Direction.performed -= Controller.ListenDirectionPerformed;
        }
    }
}