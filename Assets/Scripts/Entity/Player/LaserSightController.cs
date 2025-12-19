using System;
using UnityEngine;

namespace Entity.Player
{
    public class LaserSightController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField]private Camera playerCam;
        
        [Header("Laser Settings")]
        [SerializeField] private float laserMaxDistance = 100f;
        [SerializeField] private LayerMask hitLayers = -1;

        [Header("Laser Origins")]
        [SerializeField] private Transform laserOrigin1;
        [SerializeField] private Transform laserOrigin2;
        [SerializeField] private bool enableLasers = false;

        [Header("Laser Dots")]
        [SerializeField] private GameObject laserDotPrefab;
        [SerializeField] private float dotSize = 0.1f;
        [SerializeField] private float dotOffset = 0.01f;

        [Header("Laser Beams")]
        [SerializeField] private LineRenderer laserRenderer1;
        [SerializeField] private LineRenderer laserRenderer2;
        [SerializeField] private float beamWidth = 0.02f;
        [SerializeField] private Color beamColor = new Color(1f, 0f, 0f, 0.8f);
        [SerializeField] private Material beamMaterial;
        
        [Header("Volumetric Effect")]
        [SerializeField] private bool enableVolumetric = true;
        [SerializeField] private GameObject volumetricEffectPrefab;
        [SerializeField] private float volumetricWidth = 0.2f;
        [SerializeField] private Color volumetricColor = new Color(1f, 0f, 0f, 0.25f);
        [SerializeField] private Material volumetricMaterial;
        
        [Header("Fog Particles")]
        [SerializeField] private bool enableParticles = true;
        [SerializeField] private ParticleSystem fogParticles;
        [SerializeField] private int particlesPerMeter = 10;
        [SerializeField] private float particleSize = 0.1f;
        [SerializeField] private Color particleColor = new Color(1f, 0.5f, 0.5f, 0.3f);
        
        // laser 1
        private GameObject _laserDot1;
        private Transform _laserDot1Transform;
        private Renderer _dot1Renderer;
        private GameObject _volumetricBeam1;
        private Transform _volumetricBeam1Transform;
        
        // laser 2
        private GameObject _laserDot2;
        private Transform _laserDot2Transform;
        private Renderer _dot2Renderer;
        private GameObject _volumetricBeam2;
        private Transform _volumetricBeam2Transform;
        
        private ParticleSystem.EmitParams _emitParams;

        private void Awake()
        {
            if (playerCam == null)
            {
                Debug.LogError("No player camera assigned to LaserSightController");
            }
            
            // laser1 setup
            
        }

        // Update is called once per frame
        void Update()
        {
        
        }
    }
}
