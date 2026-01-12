using System;
using System.Collections;
using System.Collections.Generic;
using GameEvents;
using UnityEngine;
using URP;

namespace UI
{
    public class SignalJamController : MonoBehaviour
    {
        [Header("Strength Settings")]
        [SerializeField] private float maxIntensity = 1f;
        [SerializeField] private float noiseStrength = 0.6f;
        [SerializeField] private float scanlineStrength = 0.7f;
        [SerializeField] private float pixelation = 180f;
        
        [Header("Pulse Settings")]
        [SerializeField] private float pulseJamStrength = 0.3f; // Changed from 5 to 0.3
        [SerializeField] private float pulseJamDuration = 5f;

        [Header("Timings")]
        [SerializeField] private float fadeOutSpeed = 3f;

        private float _currIntensity;
        private bool _isJammed;
        
        private Coroutine _jamCoroutine;

        private static readonly int ScanlineStrength = Shader.PropertyToID("_ScanlineStrength");
        private static readonly int NoiseStrength = Shader.PropertyToID("_NoiseStrength");
        private static readonly int Intensity = Shader.PropertyToID("_Intensity");
        private static readonly int Pixelation = Shader.PropertyToID("_Pixelation");
        private static readonly int GlobalIntensity = Shader.PropertyToID("_SignalJamIntensity");
        
        private void Start()
        {
            EventHub.OnPlayerHurt += PulseJamming;
            
            // Set initial shader properties
            Shader.SetGlobalFloat(NoiseStrength, noiseStrength);
            Shader.SetGlobalFloat(ScanlineStrength, scanlineStrength);
            Shader.SetGlobalFloat(Pixelation, pixelation);
        }

        private void Awake()
        {
            _currIntensity = 0;
        }

        private void OnDestroy()
        {
            EventHub.OnPlayerHurt -= PulseJamming;
            
            _currIntensity = 0;
        }

        private void Update()
        {
            float target = _isJammed ? maxIntensity : 0f;
            
            // float prevIntensity = _currIntensity;
            _currIntensity = target;

            if (!_isJammed)
            {
                _currIntensity = Mathf.MoveTowards(_currIntensity, target, fadeOutSpeed * Time.deltaTime);
            }
            
            Shader.SetGlobalFloat(GlobalIntensity, _currIntensity);
        }

        public void EnableJamming()
        {
            _currIntensity = maxIntensity;
            _isJammed = true;
        }

        public void DisableJamming()
        {
            _isJammed = false;
        }

        public void PulseJamming(float damage)
        {
            if (_jamCoroutine != null)
            {
                return;
            }
            _jamCoroutine = StartCoroutine(HandlePulse());
        }

        private IEnumerator HandlePulse()
        {
            _isJammed = true;
            maxIntensity = Mathf.Clamp01(pulseJamStrength);
            
            yield return new WaitForSeconds(pulseJamDuration);
            
            _isJammed = false;
            
            _jamCoroutine = null;
        }
    }
}