using System;
using EventSystem;
using UnityEngine;

namespace Entity.Player
{
    // handles camera zoom behavior when interacting with terminals
    public class CameraZoomController : MonoBehaviour
    {
        [Header("References")]
        public Camera playerCam;
        public PlayerController playerController;
        public GameObject playerModel;

        [Header("Zoom Settings")]
        public float zoomDistance = 1.5f;
        public float zoomSpeed = 5f;
        [Tooltip("For when screen is offset from the center")]
        public Vector3 screenOffset = Vector3.zero;

        private bool _isTerminalOpened = false;
        private bool _isTransitioning = false;
        private float _transitionProgress = 0f;

        // Store the ORIGINAL player camera state before zooming
        private Vector3 _originalCamPosition;
        private Quaternion _originalCamRotation;

        private Vector3 _startPosition;
        private Quaternion _startRotation;
        private Vector3 _targetPosition;
        private Quaternion _targetRotation;

        private Transform _terminalScreenTransform;

        void Start()
        {
            EventHub.OnTerminalStatusChanged += HandleTerminalStatus;
        }

        private void Awake()
        {
            if (playerController == null)
            {
                Debug.LogWarning("PlayerController is null");
            }

            if (playerCam == null)
            {
                Debug.LogWarning("PlayerCam is null");
            }

            if (playerModel == null)
            {
                Debug.LogWarning("PlayerModel is null");
            }
        }

        void Update()
        {
            if (_isTransitioning)
            {
                _transitionProgress += Time.deltaTime * zoomSpeed;

                // Clamp to 0-1 range
                float t = Mathf.Clamp01(_transitionProgress);

                // Lerp from start to target using the progress value
                playerCam.transform.position = Vector3.Lerp(_startPosition, _targetPosition, t);
                playerCam.transform.rotation = Quaternion.Slerp(_startRotation, _targetRotation, t);

                // Check if transition is complete
                if (t >= 1f)
                {
                    _isTransitioning = false;
                    _transitionProgress = 0f;

                    // Re-enable camera controller when zooming OUT is complete
                    if (!_isTerminalOpened)
                    {
                        playerController.EnableMovement();
                        playerModel.SetActive(true);
                    }
                }
            }
        }

        private void HandleTerminalStatus(bool inTerminalStatus, Transform inTerminalScreenTransform)
        {
            _isTerminalOpened = inTerminalStatus;
            _terminalScreenTransform = inTerminalScreenTransform;
            HandleCameraZoom();
        }

        private void HandleCameraZoom()
        {
            _isTransitioning = true;
            _transitionProgress = 0f;

            if (_isTerminalOpened)
            {
                playerController.DisableMovement();
                playerModel.SetActive(false);

                // Store ORIGINAL position (where we'll return to)
                _originalCamPosition = playerCam.transform.position;
                _originalCamRotation = playerCam.transform.rotation;

                // Set up lerp
                _startPosition = playerCam.transform.position;
                _startRotation = playerCam.transform.rotation;

                _targetPosition = _terminalScreenTransform.position
                                  - _terminalScreenTransform.forward * zoomDistance
                                  + screenOffset;
                _targetRotation = Quaternion.LookRotation(_terminalScreenTransform.forward);
            }
            else
            {
                // === ZOOMING OUT ===
                // Set up lerp back to ORIGINAL position
                _startPosition = playerCam.transform.position;
                _startRotation = playerCam.transform.rotation;

                _targetPosition = _originalCamPosition;
                _targetRotation = _originalCamRotation;

                // Camera controller will be re-enabled after transition completes
            }
        }

        private void OnDestroy()
        {
            EventHub.OnTerminalStatusChanged -= HandleTerminalStatus;
        }
    }
}