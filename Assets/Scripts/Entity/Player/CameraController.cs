using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace Entity.Player
{
    public class CameraController : MonoBehaviour
    {
        [Header("References")]
        public Transform playerTransform;
        public Transform camPivot;
        public Transform camTransform;
        [SerializeField] private PlayerInputReader inputReader;

        [Header("Cam Settings")]
        public Vector3 normalOffset = new Vector3(0.5f, 1.4f, -2.5f);
        public Vector3 aimingOffset = new Vector3(0.3f, 1.4f, -1.2f);
        public float lookSens = 5f;
        public float pivotHeight = 1.5f;
        public float smoothSpeed = 10f;
        public float minPitch = -40f;
        public float maxPitch = 60f;

        [Header("Cam Collision")]
        public float minCollisionDistance = 0.5f;
        public float collisionRadius = 0.3f;
        public float collisionSmoothing = 0.5f;
        public LayerMask collisionLayers;

        [Header("ADS Settings")]
        public float normalFOV = 70f;
        public float adsFOV = 50f;
        public float fovTransitionSpeed = 10.0f;

        private Camera _cam;
        private float _rotationX = 0.0f;
        private float _rotationY = 0.0f;
        private Vector3 _currOffset;
        private float _currDistance;
        private bool _isAiming = false;
        private bool _isEnabled;
        private Ray _aimingRay;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            _currOffset = normalOffset;
            _currDistance = normalOffset.magnitude;

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            Vector3 angles = playerTransform.eulerAngles;
            _rotationX = angles.y;
            _rotationY = angles.x;

            _isEnabled = true;
        }

        private void Awake()
        {
            _cam = GetComponent<Camera>();
            if (_cam == null)
            {
                Debug.LogError("Camera not found");
            }

            if (inputReader == null)
            {
                Debug.LogError("Input reader not found");
            }
        }

        // Update is called once per frame
        void Update()
        {
            if (!_isEnabled)
            {
                return;
            }
            HandleInput();
        }

        void LateUpdate()
        {
            if (!_isEnabled)
            {
                return;
            }
            UpdatePivotPos();
            HandleRotation();
            HandleCamPosition();
            HandleFOV();
            HandleAiming();
        }

        private void HandleAiming()
        {
            if (!_isAiming)
            {
                return;
            }
            
            _aimingRay = new Ray(GetCameraTransform().position, GetCameraDirection());
        }

        public Ray GetRay()
        {
            return _aimingRay;
        }

        private void UpdatePivotPos()
        {
            Vector3 targetPivotPos = playerTransform.position + Vector3.up * pivotHeight;
            camPivot.position = Vector3.Lerp(
                camPivot.position,
                targetPivotPos,
                smoothSpeed * Time.deltaTime
            );
        }

        private void HandleFOV()
        {
            float targetFov = _isAiming ? adsFOV : normalFOV;
            _cam.fieldOfView = Mathf.Lerp(_cam.fieldOfView, targetFov, fovTransitionSpeed * Time.deltaTime);
        }

        private void HandleCamPosition()
        {
            Vector3 desiredLocalPos = _currOffset;
            Vector3 desiredWorldPos = camPivot.TransformPoint(desiredLocalPos);

            Vector3 direction = desiredWorldPos - camPivot.position;
            float targetDistance = direction.magnitude;

            RaycastHit camCollisionCheckHit;
            if (Physics.SphereCast(camPivot.position, collisionRadius, direction.normalized, out camCollisionCheckHit,
                    targetDistance, collisionLayers))
            {
                _currDistance = Mathf.Lerp(_currDistance,
                    Mathf.Max(camCollisionCheckHit.distance - collisionRadius, minCollisionDistance),
                    collisionSmoothing * Time.deltaTime);
            }
            else
            {
                _currDistance = Mathf.Lerp(_currDistance, targetDistance, collisionSmoothing * Time.deltaTime);
            }

            Vector3 camFinalPos = _currOffset.normalized * _currDistance;
            camTransform.localPosition = camFinalPos;
        }

        private void HandleRotation()
        {
            camPivot.rotation = Quaternion.Euler(_rotationY, _rotationX, 0f);
        }

        private void HandleInput()
        {
            Vector2 lookInput = inputReader.LookInput;
            float mouseX = lookInput.x * lookSens;
            float mouseY = lookInput.y * lookSens;

            _rotationX += mouseX;
            _rotationY -= mouseY;
            _rotationY = Mathf.Clamp(_rotationY, minPitch, maxPitch);

            _isAiming = inputReader.AimPressed;
        }

        public bool IsAiming()
        {
            return _isAiming;
        }

        public Vector3 GetCameraDirection()
        {
            return transform.forward;
        }

        public Transform GetCameraTransform()
        {
            return transform;
        }
        
        public void EnableCameraMovement()
        {
            _isEnabled = true;
        }

        public void DisableCameraMovement()
        {
            _isEnabled = false;
        }
    }
}