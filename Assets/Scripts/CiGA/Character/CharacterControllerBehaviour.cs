using System;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.InputSystem;

namespace CiGA.Character
{
    [RequireComponent(typeof(Rigidbody)), RequireComponent(typeof(CharacterControllerInput))]
    public class CharacterControllerBehaviour : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] protected Rigidbody Rigidbody;
        [SerializeField] protected Collider Collider;
        [SerializeField] protected CharacterControllerInput Input;
        [SerializeField] protected Transform GroundCheck;
        [SerializeField] protected LayerMask GroundMask;

        [Header("Attributes")] [SerializeField]
        private float groundCheckRadius = 1f;

        [SerializeField, MinValue(0)] private float jumpFactor = 7f;
        public bool IsGrounded
        {
            get { return Physics.CheckSphere(GroundCheck.position, groundCheckRadius, GroundMask); }
        }

        private float _move = 0f, _moveTarget = 0f;
        protected void Awake()
        {
            if (!Rigidbody) Rigidbody = GetComponent<Rigidbody>();
            if (!Collider) Collider = GetComponent<Collider>();
            if (!Input) Input = GetComponent<CharacterControllerInput>();
        }

        protected void Update()
        {
            MovementCore();
        }

        protected void FixedUpdate()
        {
            MovementTickCore();
        }

        #region Core Functions
        protected void MovementCore()
        {
            Rigidbody.velocity = new Vector3(_move, Rigidbody.velocity.y, 0);
        }

        protected void MovementTickCore()
        {
            _move = Mathf.Lerp(_move, _moveTarget, Time.fixedDeltaTime * 5);
        }
        #endregion
        
        #region Player Actions

        public void Jump()
        {
            Rigidbody.AddForce(Vector3.up * jumpFactor, ForceMode.Impulse);
        }

        public void Push()
        {
            
        }
        #endregion
        
        #region Input Listeners

        public void ListenMovementPerformed(InputAction.CallbackContext context)
        {
            _moveTarget = context.ReadValue<float>();
        }

        public void ListenMovementCancelled(InputAction.CallbackContext context)
        {
            _moveTarget = 0f;
        }

        public void ListenJump(InputAction.CallbackContext context)
        {
            if (IsGrounded)
                Jump();
        }

        public void ListenPush(InputAction.CallbackContext context)
        {
            Push();
        }
        #endregion
    }
}