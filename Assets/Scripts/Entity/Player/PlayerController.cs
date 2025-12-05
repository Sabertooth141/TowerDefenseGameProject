using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

namespace Entity.Player
{
    public class PlayerController : Entity
    {
        // [Header("Camera Controls")]
        // [SerializeField] private Transform cam;
        // [SerializeField] private Transform camPivot;
        // public float mouseSensitivity = 120f;
        // public float minPitch = -40f;
        // public float maxPitch = 70f;
        // public float camSnapSpd = 5.0f;
        
        [SerializeField]
        private Transform playerModelTransform;

        [Header("Movement Controls")]
        public float walkingSpeed = 10.0f;
        public float sprintSpeed = 20.0f;
        public float acceleration = 5.0f;
        public float rotationSpeed = 1.0f;

        [Header("Gravity / Jump")]
        public float gravity = -9.8f;
        public float groundCheckDist = 1.0f;
        public float jumpSpeed = 5.0f;

        private Vector2 _input;
        private float _currSpeed;
        private bool _isJumping;
        private bool _isGrounded;

        private float _pitch;
        private float _yaw;

        private PlayerInputReader _inputReader;
        private Rigidbody _rb;

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
        }

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
            _rb.useGravity = false;
            _rb.isKinematic = false;
            _rb.freezeRotation = true;

            _inputReader = GetComponent<PlayerInputReader>();

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            _currSpeed = walkingSpeed;
        }

        protected override void Update()
        {
            base.Update();
            HandleCam();
        }

        private void FixedUpdate()
        {
            GroundCheck();
            HandleGravity();
            HandleMovement();
            HandleSprint();
            HandleRotation();

            if (_inputReader.JumpPressed)
            {
                HandleJump();
            }
        }

        private void HandleRotation()
        {

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
            _isGrounded = Physics.Raycast(transform.position, Vector3.down, groundCheckDist);
        }

        private void HandleMovement()
        {
            Vector2 movementInput = _inputReader.MovementInput;

            // Vector3 camForward = cam.forward;
            // camForward.y = 0;
            // camForward.Normalize();
            //
            // Vector3 camRight = cam.right;
            // camRight.y = 0;
            // camRight.Normalize();

            Vector3 desiredDir = transform.forward * movementInput.y + transform.right * movementInput.x;
            Vector3 desiredVel = desiredDir * _currSpeed;

            Vector3 velocity = _rb.linearVelocity;
            velocity.y = 0;
            Vector3 changeVel = desiredVel - velocity;

            if (_inputReader.MovementInput.sqrMagnitude > 0.01f)
            {
                Quaternion newRotation = Quaternion.LookRotation(desiredDir);
                playerModelTransform.rotation =  Quaternion.Slerp(newRotation, playerModelTransform.rotation, rotationSpeed * Time.deltaTime);
            }
            
            _rb.AddForce(changeVel * acceleration, ForceMode.Acceleration);
        }

        private void HandleCam()
        {

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
    }
}