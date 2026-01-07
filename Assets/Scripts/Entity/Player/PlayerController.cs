using System;
using System.Collections;
using System.Collections.Generic;
using Entity.Turret;
using EventSystem;
using Terminal;
using TMPro;
using UI;
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
        [SerializeField] private UIController uiController;
        [SerializeField] private Transform spawnPoint;

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
        
        [Header("ADS Rotation Handling")]
        public LayerMask adsRotationMask;

        private Vector2 _input;
        private RaycastHit _slopeHit;

        private bool _isJumping;
        private bool _isGrounded;
        private bool _onSlope;
        private bool _canMove;
        private bool _canLook;

        private float _currSpeed;
        private Vector3 _moveDirection;
        private Vector3 _currentVelocity;

        //check ground param
        private const float GroundRayOffset = 0.1f;
        private const float GroundSphereYOffset = 0.1f;
        private const float GroundSphereROffset = 0.9f;
        //check slope param
        private const float SlopeCheckOffset = 0.5f;

        // reference
        private PlayerInputReader _inputReader;
        private Rigidbody _rb;
        private CapsuleCollider _capsuleCollider;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        protected override void Start()
        {
            base.Start();

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            _currSpeed = walkingSpeed;

            _canMove = true;
            _canLook = true;
        }

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
            _inputReader = GetComponent<PlayerInputReader>();
            _capsuleCollider = GetComponentInChildren<CapsuleCollider>();

            _rb.useGravity = false;
            _rb.isKinematic = false;
            _rb.freezeRotation = true;

            NullCheck();
        }

        protected override void Update()
        {
            base.Update();

            HandleRotation();
        }

        private void FixedUpdate()
        {
            if (!_canMove)
            {
                return;
            }

            GroundCheck();
            HandleGravity();
            HandleMovement();
            CheckSlope();

            if (_inputReader.JumpPressed)
            {
                HandleJump();
            }
        }

        private void NullCheck()
        {
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

            if (_capsuleCollider == null)
            {
                Debug.LogError("PlayerController: CapsuleCollider not found");
            }

            if (spawnPoint == null)
            {
                Debug.LogError("PlayerController: SpawnPoint not found");
            }
        }
        
        private void HandleRotation()
        {
            if (!_canLook)
            {
                return;
            }

            bool isAiming = cameraController != null && cameraController.IsAiming();

            if (isAiming)
            {
                RaycastHit cameraAimingPoint;
                Physics.Raycast(cameraController.GetRay(), out cameraAimingPoint, adsRotationMask);

                Vector3 lookDirection = (cameraAimingPoint.point - transform.position).normalized;
                // lookDirection.y = 0;

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
                    _capsuleCollider.height * 0.5f + SlopeCheckOffset))
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
                if (newVelocity.y >= maxFallSpeed)
                {
                    newVelocity.y = maxFallSpeed;
                }
            }

            _rb.linearVelocity = newVelocity;
        }

        private void GroundCheck()
        {
            Vector3 rayStart = transform.position;
            float rayLength = groundCheckDist;
    
            // Simple raycast from center
            _isGrounded = Physics.Raycast(rayStart, Vector3.down, rayLength, groundMask);
    
            // OR use sphere cast for better detection
            // _isGrounded = Physics.SphereCast(rayStart, _capsuleCollider.radius * 0.9f, 
            //                                   Vector3.down, out _, rayLength, groundMask);
    
            // Debug visualization
            Debug.DrawRay(rayStart, Vector3.down * rayLength, 
                _isGrounded ? Color.green : Color.red);
        }

        private void OnDrawGizmos()
        {
            if (_capsuleCollider == null) return;

            Vector3 spherePosition = transform.position + Vector3.down * 0.7f;

            // Set color based on grounded state
            Gizmos.color = _isGrounded ? Color.green : Color.red;

            // Draw the sphere
            Gizmos.DrawWireSphere(spherePosition, _capsuleCollider.radius);
        }

        private void HandleMovement()
        {
            Vector2 movementInput = _inputReader.MovementInput;

            if (movementInput.magnitude < 0.01f)
            {
                // Apply drag when no input
                Vector3 velocity = _rb.linearVelocity;
                velocity.x *= Mathf.Lerp(1f, 0f, deceleration * Time.fixedDeltaTime);
                velocity.z *= Mathf.Lerp(1f, 0f, deceleration * Time.fixedDeltaTime);
                _rb.linearVelocity = velocity;
                return;
            }

            // Calculate movement direction
            Vector3 inputDir = new Vector3(movementInput.x, 0, movementInput.y).normalized;
            Vector3 camForward = cameraTransform.forward;
            Vector3 camRight = cameraTransform.right;

            camForward.y = 0;
            camRight.y = 0;
            camForward.Normalize();
            camRight.Normalize();

            _moveDirection = camForward * inputDir.z + camRight * inputDir.x;

            // Set speed
            if (cameraController != null && cameraController.IsAiming())
            {
                _currSpeed = aimWalkingSpeed;
            }
            else
            {
                _currSpeed = walkingSpeed;
            }
            HandleSprint();

            // Calculate target velocity
            Vector3 targetVelocity = _moveDirection * _currSpeed;

            // Apply movement
            if (_isGrounded)
            {
                // Handle slope
                if (_onSlope)
                {
                    targetVelocity = Vector3.ProjectOnPlane(targetVelocity, _slopeHit.normal);
                    _rb.AddForce(Vector3.down * slopeForce, ForceMode.Force);
                }

                // Smooth velocity change
                Vector3 currentHorizVelocity = new Vector3(_rb.linearVelocity.x, 0, _rb.linearVelocity.z);
                Vector3 newHorizVelocity = Vector3.Lerp(currentHorizVelocity, targetVelocity,
                    acceleration * Time.fixedDeltaTime);

                _rb.linearVelocity = new Vector3(newHorizVelocity.x, _rb.linearVelocity.y, newHorizVelocity.z);
            }
            else
            {
                // Air movement (optional - reduced control)
                Vector3 currentHorizVelocity = new Vector3(_rb.linearVelocity.x, 0, _rb.linearVelocity.z);
                Vector3 newHorizVelocity = Vector3.Lerp(currentHorizVelocity, targetVelocity,
                    acceleration * 0.5f * Time.fixedDeltaTime);

                _rb.linearVelocity = new Vector3(newHorizVelocity.x, _rb.linearVelocity.y, newHorizVelocity.z);
            }
        }

        private void HandleJump()
        {
            if (_isJumping)
            {
                return;
            }

            Vector3 newVelocity = _rb.linearVelocity;
            newVelocity.y = jumpSpeed;
            _rb.linearVelocity = newVelocity;
            _isJumping = true;
        }

        private void OnTriggerStay(Collider other)
        {
            if (other.CompareTag("Terminal") || other.CompareTag("ReactorTerminal"))
            {
                uiController.EnableInteraction("[F] Open Terminal");
                if (_inputReader.InteractPressed)
                {
                    uiController.DisableInteraction();
                    if (other.GetComponent<TerminalController>() != null)
                    {
                        other.GetComponent<TerminalController>().StartTerminal();
                    }

                    DisableMovement();
                }
            }

            if (other.CompareTag("TurretInteraction"))
            {
                uiController.EnableInteraction("[F] Destroy Turret");
                if (_inputReader.InteractPressed)
                {
                    uiController.DisableInteraction();
                    other.GetComponentInParent<TurretController>().DisableTurret();
                }
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("ResetCollider"))
            {
                StartCoroutine(MoveToSpawnPoint());
            }
        }

        private IEnumerator MoveToSpawnPoint()
        {
            Transform currTransform = transform;
            
            DisableMovement();
            _rb.isKinematic = true;

            while (Vector3.Distance(currTransform.position, spawnPoint.position) > 0.01f)
            {
                currTransform.position = Vector3.MoveTowards(currTransform.position, spawnPoint.position, walkingSpeed * Time.deltaTime);
                
                yield return null;
            }
            
            currTransform.position = spawnPoint.position;
            
            EnableMovement();
            _rb.isKinematic = false;
        }

        private void OnTriggerExit(Collider other)
        {
            uiController.DisableInteraction();
        }

        public void DisableMovement()
        {
            _canMove = false;
            _canLook = false;
            cameraController.DisableCameraMovement();
        }

        public void EnableMovement()
        {
            _canMove = true;
            _canLook = true;
            cameraController.EnableCameraMovement();
        }

        public override void TakeDamage(float damage)
        {
            base.TakeDamage(damage);
            
            Debug.Log("TAKE DAMAGE");
        }
    }
}