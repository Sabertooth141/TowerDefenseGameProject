using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Entity.Player
{
    public class PlayerInputReader : MonoBehaviour
    {
        public Vector2 MovementInput { get; private set; }
        public Vector2 LookInput { get; private set; }
        public bool JumpPressed { get; private set; }
        public bool SprintPressed { get; private set; }
        public bool AimPressed { get; private set; }
        public bool ShootPressed  { get; private set; }

        private PlayerControls _controls;

        private void Awake()
        {
            _controls = new PlayerControls();
            
            _controls.Player.Move.performed += ctx => MovementInput = ctx.ReadValue<Vector2>();
            _controls.Player.Move.canceled += ctx => MovementInput = Vector2.zero;
            
            _controls.Player.Look.performed += ctx => LookInput = ctx.ReadValue<Vector2>();
            _controls.Player.Look.canceled += ctx => LookInput = Vector2.zero;
            
            _controls.Player.Jump.performed += ctx => JumpPressed = true;
            _controls.Player.Jump.canceled += ctx => JumpPressed = false;
            
            _controls.Player.Sprint.performed += ctx => SprintPressed = true;
            _controls.Player.Sprint.canceled += ctx => SprintPressed = false;
            
            _controls.Player.Aim.performed += ctx => AimPressed = true;
            _controls.Player.Aim.canceled += ctx => AimPressed = false;
            
            _controls.Player.Shoot.performed += ctx => ShootPressed = true;
            _controls.Player.Shoot.canceled += ctx => ShootPressed = false;
        }

        private void OnEnable()
        {
            _controls.Player.Enable();
        }

        private void OnDisable()
        {
            _controls.Player.Disable();
        }
    }
}