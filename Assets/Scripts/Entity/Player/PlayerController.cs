using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Entity.Player
{
    public class PlayerController : Entity
    {
        [SerializeField] private Transform cam;

        public float walkingSpeed = 5.0f;
        public float runningSpeed = 10.0f;
        public float jumpSpeed = 5.0f;
        public float gravity = -9.8f;
        public float groundCheckDist = 1.0f;
        
        private Vector2 _input;
        private bool _isJumping;
        private bool _isGrounded;
        
        private PlayerInputReader _inputReader;
        private Rigidbody _rb;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        protected override void Start()
        {
            base.Start();
            if (cam == null)
            {
                Debug.LogError("PlayerController: PlayerCamera needs to be assigned");
            }

            _rb = GetComponent<Rigidbody>();
            if (_rb == null)
            {
                Debug.LogError("PlayerController: PlayerRigidBody not found");
            }
            
            _inputReader = GetComponent<PlayerInputReader>();
            if (_inputReader == null)
            {
                Debug.LogError("PlayerController: PlayerInput reader not found");
            }
        }

        protected override void Update()
        {
            base.Update();
        }

        private void FixedUpdate()
        {
            GroundCheck();
            HandleGravity();
            HandleMovement();

            if (_inputReader.JumpPressed)
            {
                HandleJump();
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