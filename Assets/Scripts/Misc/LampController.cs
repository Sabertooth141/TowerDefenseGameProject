using System.Collections;
using EventSystem;
using UnityEngine;

namespace Misc
{
    public class LampController : MonoBehaviour
    {
        [SerializeField] private float lightIntensity = 10f;
        [SerializeField] private Material lightsOffMat;

        private Light _light;
        private MeshRenderer _lampRenderer;
        private Material _lampMaterial;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            EventHub.OnGeneratorStart += TurnOnLight;
        }

        private void Awake()
        {
            _light = GetComponentInChildren<Light>();
            _lampRenderer = GetComponent<MeshRenderer>();

            if (_light == null)
            {
                Debug.LogError("LampController: Lamp lighting is missing");
            }

            if (_lampRenderer == null)
            {
                Debug.LogError("LampController: Lamp renderer is missing");
            }

            if (lightsOffMat == null)
            {
                Debug.LogError("LampController: Lamp lights off material is missing");
            }
            
            _lampMaterial = _lampRenderer.sharedMaterials[1];
            TurnOffLight();
        }

        private void TurnOffLight()
        {
            var mats = _lampRenderer.sharedMaterials;
            mats[1] = lightsOffMat;
            _lampRenderer.sharedMaterials = mats;

            _light.enabled = false;
        }

        private void TurnOnLight()
        {
            StartCoroutine(StartLightUp());
            _lampRenderer.enabled = true;
        }

        private IEnumerator StartLightUp()
        {
            _light.intensity = 0;
            _light.enabled = true;

            var mats = _lampRenderer.sharedMaterials;
            mats[1] = _lampMaterial;
            _lampRenderer.sharedMaterials = mats;

            float lightIntensityCounter = 0f;
            while (lightIntensityCounter < lightIntensity)
            {
                lightIntensityCounter += Time.deltaTime;
                _light.intensity = lightIntensityCounter;
                yield return null;
            }
        }

    }
}