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
        [SerializeField] private float speed = 4f;
        [SerializeField, MinValue(0)] private float jumpFactor = 7f;
        
        [Header("Push Mechanics")]
        [SerializeField] private float pushDistance = 1.5f;
        [SerializeField] private float pushForce = 10f;
        [SerializeField] private LayerMask pushableLayers;
        
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
            Rigidbody.velocity = new Vector3(_move * speed, Rigidbody.velocity.y, 0);
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

        public void Push(InputAction.CallbackContext context)
        {
            if (context.started)
            {
                // Try to grab an object when the button is first pressed
                TryGrabObject();
            }
            else if (context.canceled)
            {
                // Release the object when the button is released
                ReleaseObject();
            }
            else if (context.performed && heldObject != null)
            {
                // Continue pushing while the button is held
                PushHeldObject();
            }
        }

        private void TryGrabObject()
        {
            RaycastHit hit;
            if (Physics.Raycast(transform.position, transform.right, out hit, pushDistance, pushableLayers))
            {
                heldObject = hit.rigidbody;
                if (heldObject != null)
                {
                    holdOffset = heldObject.transform.position - transform.position;
                    holdOffset = Vector3.ProjectOnPlane(holdOffset, Vector3.forward);
                }
            }
        }

        private void ReleaseObject()
        {
            heldObject = null;
        }

        private void PushHeldObject()
        {
            if (heldObject != null)
            {
                Vector3 targetPosition = transform.position + holdOffset;
                Vector3 pushDirection = (targetPosition - heldObject.position);
                
                // Apply force to move the object towards the hold position
                heldObject.AddForce(pushDirection * pushForce, ForceMode.Force);
                
                // Limit the velocity to prevent the object from moving too fast
                heldObject.velocity = Vector3.ClampMagnitude(heldObject.velocity, speed * 1.2f);
            }
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
            Push(context);
        }

        #endregion

        private void OnDrawGizmos()
        {
            // Visualize push distance
            Gizmos.color = Color.yellow;
            Gizmos.DrawRay(transform.position, transform.right * pushDistance);
        }
        #endregion
    }
}