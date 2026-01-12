using System;
using GameEvents;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Interactions;
using UnityEngine.UI;

namespace UI
{
    public class HoldToInteract : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Slider holdSlider;

        [Header("Settings")]
        [SerializeField] private float holdDuration = 0.5f;
        [SerializeField] private float sliderResetTime = 0.3f;

        private float _currentHoldTime = 0f; // Accumulate time with deltaTime
        private bool _isHolding = false;
        private bool _canInteract = false;

        private void Awake()
        {
            if (holdSlider != null)
            {
                holdSlider.maxValue = 1;
                holdSlider.minValue = 0;
                holdSlider.value = 0;
                holdSlider.gameObject.SetActive(false);
            }
        }

        private void Start()
        {
            EventHub.OnEnableInteract += EnableInteract;
            EventHub.OnDisableInteract += DisableInteract;
        }

        private void OnDestroy()
        {
            EventHub.OnEnableInteract -= EnableInteract;
            EventHub.OnDisableInteract -= DisableInteract;
        }

        private void EnableInteract()
        {
            _canInteract = true;
        }

        private void DisableInteract()
        {
            _canInteract = false;
            ResetSlider();
        }

        public void OnStartedInteract(InputAction.CallbackContext context)
        {
            if (!_canInteract)
            {
                return;
            }

            if (holdSlider == null)
            {
                return;
            }
            if (context.interaction is HoldInteraction)
            {
                _isHolding = true;
                _currentHoldTime = 0f; // Reset accumulator

                holdSlider.gameObject.SetActive(true);
            }
        }

        public void OnPerformedInteract(InputAction.CallbackContext context)
        {
            if (!_canInteract)
            {
                return;
            }
            
            if (context.interaction is HoldInteraction)
            {
                CompleteInteraction();
            }
        }

        public void OnCanceledInteract(InputAction.CallbackContext context)
        {
            if (!_canInteract)
            {
                return;
            }
            
            if (holdSlider == null)
            {
                return;
            }
            if (context.interaction is HoldInteraction)
            {
                _isHolding = false;
                ResetSlider();
            }
        }

        private void CompleteInteraction()
        {
            if (!_canInteract)
            {
                return;
            }
            
            if (holdSlider == null)
            {
                return;
            }

            _isHolding = false;

            holdSlider.value = 1;
            Invoke(nameof(ResetSlider), sliderResetTime);
        }

        private void ResetSlider()
        {
            if (holdSlider == null)
            {
                return;
            }

            _currentHoldTime = 0f;
            holdSlider.value = 0;
            holdSlider.gameObject.SetActive(false);
        }

        private void Update()
        {
            if (!_isHolding || holdSlider == null || !_canInteract)
            {
                return;
            }

            _currentHoldTime += Time.deltaTime; // Accumulate time each frame
            float progress = Mathf.Clamp01(_currentHoldTime / holdDuration);
            holdSlider.value = progress;
        }
    }
}