using System;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

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
        [SerializeField] protected CharacterBallHolder BallHolder;
        [SerializeField] protected LayerMask GroundMask;

        [Header("Attributes")] [SerializeField]
        private float groundCheckRadius = 1f;
        [SerializeField] private float speed = 4f;
        [SerializeField, MinValue(0)] private float jumpFactor = 7f;
        
        [Header("Push Mechanics")]
        [SerializeField] private float pushDistance = 1.5f;
        [SerializeField] private float maxPushForce = 10f;
        [SerializeField] public LayerMask pushableLayers;
        [SerializeField] private float aimedAngle = 0f;
        
        public bool IsGrounded
        {
            get { return Physics.CheckSphere(GroundCheck.position, groundCheckRadius, GroundMask); }
        }
        public bool IsHolding { get; set; }

        private float _move = 0f, _moveTarget = 0f;
        private Rigidbody _heldObject;
        private Vector3 _holdOffset;
        private RigidbodyConstraints _initialConstraint;
        
        protected void Awake()
        {
            if (!Rigidbody) Rigidbody = GetComponent<Rigidbody>();
            if (!Collider) Collider = GetComponent<Collider>();
            if (!Input) Input = GetComponent<CharacterControllerInput>();

            _initialConstraint = Rigidbody.constraints;
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

        public void Push()
        {
            TryGrabObject();
            if (_heldObject)
            {
                Vector3 targetPosition = transform.position + _holdOffset;
                Vector3 pushDirection = (targetPosition - _heldObject.position);
                
                // Apply force to move the object towards the hold position
                float clampedForce = Math.Clamp(maxPushForce, 0, maxPushForce * Mathf.Cos(aimedAngle));
                _heldObject.AddForce(pushDirection * clampedForce, ForceMode.Impulse);
                
                ReleaseObject();
            }
        }

        private void TryGrabObject()
        {
            RaycastHit hit;
            if (Physics.Raycast(BallHolder.transform.position, BallHolder.transform.right, out hit, pushDistance, pushableLayers))
            {
                _heldObject = hit.rigidbody;
                if (_heldObject)
                {
                    Debug.Log("Grabbed");
                    Vector3 temp = _heldObject.transform.position - transform.position;
                    temp.x += 0.5f;
                    _holdOffset = temp;
                    _holdOffset = Vector3.ProjectOnPlane(_holdOffset, Vector3.forward);
                }
            }
        }

        private void ReleaseObject()
        {
            _heldObject = null;
        }

        public void OnBallHolderCollide()
        {
            IsHolding = true;
            Rigidbody.constraints = RigidbodyConstraints.FreezeAll;
        }
        #endregion
        
        #region Input Listeners

        public void ListenMovementPerformed(InputAction.CallbackContext context)
        {
            _moveTarget = context.ReadValue<float>();
            BallHolder.gameObject.SetActive(true);
        }

        public void ListenMovementCancelled(InputAction.CallbackContext context)
        {
            _moveTarget = 0f;
            BallHolder.gameObject.SetActive(false);
            IsHolding = false;
            Rigidbody.constraints = _initialConstraint;
        }

        public void ListenJump(InputAction.CallbackContext context)
        {
            if (IsGrounded)
                Jump();
        }

        public void ListenPushPerformed(InputAction.CallbackContext context)
        {
            Push();
        }

        public void ListenPushCanceled(InputAction.CallbackContext context)
        {
            
        }

        public void ListenDirectionPerformed(InputAction.CallbackContext context)
        {
            Vector2 vec = context.ReadValue<Vector2>();
            if (vec.x < 0) return;
            aimedAngle = Mathf.Tan(Mathf.Clamp(Mathf.Abs(vec.y), 0, 1) / Mathf.Clamp(Mathf.Abs(vec.x), 0, 1)) * Mathf.Rad2Deg;
        }

        private void OnDrawGizmos()
        {
            // Visualize push distance
            Gizmos.color = Color.yellow;
            Gizmos.DrawRay(transform.position, transform.right * pushDistance);
        }
        
        public float CalculateSlopeForce()
        {
            RaycastHit hit;
            if (Physics.Raycast(transform.position, Vector3.down, out hit, 2))
            {
                Vector3 slopeNormal = hit.normal;
                float slopeAngle = Vector3.Angle(slopeNormal, Vector3.up);
                float angleFromPerpendicular = 90f - slopeAngle;
                
                return slopeAngle;
            }

            return 0f;
        }
        #endregion
    }
}