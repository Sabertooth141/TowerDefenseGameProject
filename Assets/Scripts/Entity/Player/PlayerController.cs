using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

namespace Entity.Player
{
    public class PlayerController : Entity
    {
        [Header("References")]
        [SerializeField] private Transform playerModelTransform;
        [SerializeField] private Transform cameraTransform;
        [SerializeField] private CameraController cameraController;
        [SerializeField] private CapsuleCollider capsuleCollider;

        [Header("Movement Controls")]
        public float walkingSpeed = 10.0f;
        public float aimWalkingSpeed = 8.0f;
        public float sprintSpeed = 20.0f;
        public float acceleration = 5.0f;
        public float deceleration = 10.0f;
        public float rotationSpeed = 1.0f;

        [Header("Gravity / Jump")]
        public float gravity = -9.8f;
        public float groundCheckDist = 1.0f;
        public float jumpSpeed = 5.0f;
        public float maxFallSpeed = 20.0f;
        public LayerMask groundMask;

        [Header("Slope Handling")]
        public float maxSlopeAngle = 45.0f;
        public float slopeForce = 8.0f;
        public float drag = 5.0f;

        private Vector2 _input;
        private RaycastHit _slopeHit;

        private bool _isJumping;
        private bool _isGrounded;
        private bool _onSlope;

        private float _currSpeed;
        private Vector3 _moveDirection;
        private Vector3 _currentVelocity;

        //check ground param
        private const float GroundRayOffset = 0.1f;
        private const float GroundSphereYOffset = 0.1f;
        private const float GroundSphereROffset = 0.9f;
        //check slope param
        private const float SlopeCheckOffset = 0.5f;

        private PlayerInputReader _inputReader;
        private Rigidbody _rb;
        // private CapsuleCollider _capsuleCollider;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        protected override void Start()
        {
            base.Start();

            if (playerModelTransform == null)
            {
                Debug.LogError("PlayerController: PlayerModelTransform is null");
            }

            if (_rb == null)
            {
                Debug.LogError("PlayerController: PlayerRigidBody not found");
            }

            if (_inputReader == null)
            {
                Debug.LogError("PlayerController: PlayerInput reader not found");
            }

            if (cameraTransform == null)
            {
                Debug.LogError("PlayerController: CameraTransform not found");
            }
        }

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
            _inputReader = GetComponent<PlayerInputReader>();
            // _capsuleCollider = GetComponent<CapsuleCollider>();

            _rb.useGravity = false;
            _rb.isKinematic = false;
            _rb.freezeRotation = true;

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            _currSpeed = walkingSpeed;
        }

        protected override void Update()
        {
            base.Update();

            HandleRotation();
        }

        private void FixedUpdate()
        {
            GroundCheck();
            HandleGravity();
            HandleMovement();
            CheckSlope();

            if (_inputReader.JumpPressed)
            {
                HandleJump();
            }
        }

        private void HandleRotation()
        {
            bool isAiming = cameraController != null && cameraController.IsAiming();

            if (isAiming)
            {
                Vector3 lookDirection = cameraTransform.forward;
                lookDirection.y = 0;

                if (lookDirection != Vector3.zero)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(lookDirection);

                    playerModelTransform.rotation = Quaternion.Lerp(playerModelTransform.rotation, targetRotation,
                        rotationSpeed * Time.deltaTime);
                }
            }
            else if (_moveDirection != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(_moveDirection);

                playerModelTransform.rotation = Quaternion.Lerp(playerModelTransform.rotation, targetRotation,
                    rotationSpeed * Time.deltaTime);
            }
        }

        private void CheckSlope()
        {
            if (Physics.Raycast(transform.position, Vector3.down, out _slopeHit,
                    capsuleCollider.height * 0.5f + SlopeCheckOffset))
            {
                float slopeAngle = Vector3.Angle(Vector3.up, _slopeHit.normal);
                _onSlope = slopeAngle > 0.1f && slopeAngle <= maxSlopeAngle;
            }
            else
            {
                _onSlope = false;
            }
        }

        private void HandleSprint()
        {
            if (_inputReader.SprintPressed)
            {
                if (_isGrounded)
                {
                    _currSpeed = sprintSpeed;
                }
            }
            else
            {
                if (_isGrounded)
                {
                    _currSpeed = walkingSpeed;
                }
            }
        }

        private void HandleGravity()
        {
            Vector3 newVelocity = _rb.linearVelocity;

            if (_isGrounded && newVelocity.y < -0.0f)
            {
                _isJumping = false;
                newVelocity.y = -2.0f;
            }
            else
            {
                newVelocity.y += gravity * Time.deltaTime;
            }

            _rb.linearVelocity = newVelocity;
        }

        private void GroundCheck()
        {
            Vector3 rayStart = transform.position + Vector3.up * GroundRayOffset;
            float rayLength = groundCheckDist + GroundRayOffset;
    
            _isGrounded = Physics.Raycast(rayStart, Vector3.down, out RaycastHit hit, rayLength, groundMask);

            _isGrounded |= Physics.CheckSphere(
                transform.position + Vector3.down * 0.7f,
                capsuleCollider.radius,
                groundMask);
            
            if (_isGrounded)
            {
                Debug.DrawRay(rayStart, Vector3.down * hit.distance, Color.green);
                Debug.Log($"Hit: {hit.collider.gameObject.name} at distance {hit.distance}");
            }
            else
            {
                Debug.DrawRay(rayStart, Vector3.down * rayLength, Color.red);
                Debug.Log("Ground raycast MISSED");
            }

        }
        
        private void OnDrawGizmos()
        {
            if (capsuleCollider == null) return;
    
            Vector3 spherePosition = transform.position + Vector3.down * 0.7f;
    
            // Set color based on grounded state
            Gizmos.color = _isGrounded ? Color.green : Color.red;
    
            // Draw the sphere
            Gizmos.DrawWireSphere(spherePosition, capsuleCollider.radius);
        }

        private void HandleMovement()
        {
            Vector2 movementInput = _inputReader.MovementInput;
            Vector3 inputDir = new Vector3(movementInput.x, 0, movementInput.y).normalized;

            // get movement velocity
            if (inputDir.magnitude >= 0.01f)
            {
                Vector3 camForward = cameraTransform.forward;
                Vector3 camRight = cameraTransform.right;

                camForward.y = 0;
                camRight.y = 0;

                camForward.Normalize();
                camRight.Normalize();

                _moveDirection = camForward * inputDir.z + camRight * inputDir.x;

                //TODO: aiming
                bool isAiming = cameraController != null && cameraController.IsAiming();

                if (isAiming)
                {
                    _currSpeed = aimWalkingSpeed;
                }

                HandleSprint();
            }
            else
            {
                _moveDirection = Vector3.zero;
                _currSpeed = 0f;
            }

            Vector3 targetVelocity = _moveDirection * _currSpeed;

            // movement handling
            if (_isGrounded)
            {
                if (_onSlope && _moveDirection.sqrMagnitude > 0.01f)
                {
                    targetVelocity = Vector3.ProjectOnPlane(targetVelocity, _slopeHit.normal);
                    _rb.AddForce(Vector3.down * slopeForce, ForceMode.Force);
                }

                float accel = _moveDirection.magnitude > 0.1f ? acceleration : deceleration;

                _currentVelocity = Vector3.Lerp(new Vector3(_rb.linearVelocity.x, 0, _rb.linearVelocity.z),
                    targetVelocity, accel * Time.deltaTime);

                _rb.linearVelocity = new Vector3(_currentVelocity.x, _rb.linearVelocity.y, _currentVelocity.z);
                Vector3 velocity = _rb.linearVelocity;
                velocity.x *= 1f - drag * Time.fixedDeltaTime;
                velocity.z *= 1f - drag * Time.fixedDeltaTime;
                _rb.linearVelocity = velocity;
            }
            else
            {
            }
        }

        private void HandleJump()
        {
            // if (_isJumping)
            // {
            //     return;
            // }
            //
            // Vector3 newVelocity = _rb.linearVelocity;
            // newVelocity.y = jumpSpeed;
            // _rb.linearVelocity = newVelocity;
            // _isJumping = true;
        }
    }
}