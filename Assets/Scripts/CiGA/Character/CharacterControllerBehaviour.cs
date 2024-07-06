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
        public bool CanHold { get; set; } = true;

        private float _move = 0f, _moveTarget = 0f;
        private Rigidbody _heldObject;
        private Vector3 _holdOffset;
        private RigidbodyConstraints _initialConstraint, _otherConstraint;
        
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
            // TryGrabObject();
            Debug.Log($"{_heldObject}, Push");
            TryGrabObject();
            if (_heldObject)
            {
                Vector3 targetPosition = transform.position + _holdOffset;
                Vector3 pushDirection = (targetPosition - _heldObject.position);
                
                // Apply force to move the object towards the hold position
                float clampedForce = Math.Clamp(maxPushForce, 0, maxPushForce * Mathf.Cos(aimedAngle));
                _heldObject.AddForce(CalculateSlopeDirection(aimedAngle) * maxPushForce, ForceMode.Impulse);
                
                ReleaseObject();
            }
        }

        private void TryGrabObject()
        {
            if (_heldObject) return;
            RaycastHit hit;
            if (Physics.Raycast(BallHolder.transform.position, BallHolder.transform.right, out hit, pushDistance, pushableLayers))
            {
                _heldObject = hit.rigidbody;
            }
        }

        private void ReleaseObject()
        {
            _heldObject = null;
        }

        public void OnBallHolderCollide(Collision other)
        {
            if (!CanHold) return;
            IsHolding = true;
            Rigidbody.constraints = RigidbodyConstraints.FreezeAll;
            _heldObject = other.rigidbody;
            _heldObject.velocity = Vector3.zero;
            Debug.Log("Holding");
        }
        
        public void OnBallHolderExit(Collision other)
        {
            Rigidbody otherRigid = other.gameObject.GetComponent<Rigidbody>();
            otherRigid.constraints = _otherConstraint;
            ReleaseObject();
            Debug.Log("Holding");
        }
        #endregion
        
        #region Input Listeners

        public void ListenMovementPerformed(InputAction.CallbackContext context)
        {
            _moveTarget = context.ReadValue<float>();
            BallHolder.gameObject.SetActive(true);
            // if(Physics.ComputePenetration(BallHolder.Collider, BallHolder.transform.position, BallHolder.transform.rotation,
            //        colliderB, transformB.position, transformB.rotation,
            //        out direction, out distance))
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
        
        private Vector3 CalculateSlopeDirection(float slopeAngleDegrees)
        {
            // Clamp the angle between 0 and 90 degrees
            slopeAngleDegrees = Mathf.Clamp(slopeAngleDegrees, -90f, 90f);

            // Convert degrees to radians
            float slopeAngleRadians = slopeAngleDegrees * Mathf.Deg2Rad;
            
            return new Vector3(Mathf.Cos(slopeAngleRadians), Mathf.Sin(slopeAngleRadians), 0);
        }
        #endregion

        private void OnCollisionEnter(Collision other)
        {
            // Debug.Log($"Fuck my life {other.gameObject.layer}, {pushableLayers.value}");
            if (other.collider.CompareTag("Pushable"))
            {
                OnBallHolderCollide(other);
            }
        }

        private void OnCollisionExit(Collision other)
        {
            if (other.collider.CompareTag("Pushable"))
            {
                OnBallHolderExit(other);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.CompareTag("Pushable"))
            {
                CanHold = false;
            }
        }
        private void OnTriggerExit(Collider other)
        {
            if (other.gameObject.CompareTag("Pushable"))
            {
                CanHold = true;
            }
        }
    }
}