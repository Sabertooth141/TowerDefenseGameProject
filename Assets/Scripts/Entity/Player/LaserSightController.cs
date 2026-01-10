using UnityEngine;

namespace Entity.Player
{
    public class LaserSightController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private CameraController cam;
        [SerializeField] private GameObject laserDotPrefab;
        
        [Header("Laser Settings")]
        [SerializeField] private float laserMaxDistance = 100f;
        [SerializeField] private LayerMask hitLayers = -1;
    
        [Header("Laser Origins")]
        [SerializeField] private Transform laserOrigin1; // Left weapon/barrel
        [SerializeField] private Transform laserOrigin2; // Right weapon/barrel
        [SerializeField] private bool enableLaser1 = true;
        [SerializeField] private bool enableLaser2 = true;
    
        [Header("Laser Beams")]
        [SerializeField] private LineRenderer lineRenderer1;
        [SerializeField] private LineRenderer lineRenderer2;
        [SerializeField] private float beamWidth = 0.02f;
        [SerializeField] private Color beamColor = new Color(1f, 0f, 0f, 0.8f);
        [SerializeField] private Material beamMaterial;
    
        [Header("Volumetric Effect (Fog Rays)")]
        [SerializeField] private bool enableVolumetric = true;
        [SerializeField] private GameObject volumetricBeamPrefab;
        [SerializeField] private float volumetricWidth = 0.15f;
        [SerializeField] private Color volumetricColor = new Color(1f, 0f, 0f, 0.15f);
        [SerializeField] private Material volumetricMaterial;
    
        [Header("Animated Fog Particles")]
        [SerializeField] private bool enableParticles = true;
        [SerializeField] private ParticleSystem fogParticles;
        [SerializeField] private int particlesPerMeter = 10;
        [SerializeField] private float particleSize = 0.05f;
        [SerializeField] private Color particleColor = new Color(1f, 0.5f, 0.5f, 0.3f);
        [SerializeField] private float particleLifeTime = 0.2f;
    
        // Laser 1 components
        private GameObject _volumetricBeam1;
        private Transform _volumetric1Transform;
    
        // Laser 2 components
        private GameObject _volumetricBeam2;
        private Transform _volumetric2Transform;
    
        // Shared particle system
        private ParticleSystem.EmitParams _emitParams;

        private GameObject _laserDot1;
        private GameObject _laserDot2;

        void Start()
        {
            // Setup Laser 1
            if (enableLaser1)
            {
                if (lineRenderer1 == null)
                {
                    GameObject lr1Obj = new GameObject("LaserBeam1");
                    lr1Obj.transform.SetParent(transform);
                    lineRenderer1 = lr1Obj.AddComponent<LineRenderer>();
                }
                SetupLineRenderer(lineRenderer1);
            
                if (enableVolumetric)
                {
                    _volumetricBeam1 = CreateVolumetricBeam();
                    if (_volumetricBeam1 != null)
                    {
                        _volumetric1Transform = _volumetricBeam1.transform;
                    }
                }
                
                _laserDot1 = Instantiate(laserDotPrefab);
                _laserDot1.SetActive(false);
            }
        
            // Setup Laser 2
            if (enableLaser2)
            {
                if (lineRenderer2 == null)
                {
                    GameObject lr2Obj = new GameObject("LaserBeam2");
                    lr2Obj.transform.SetParent(transform);
                    lineRenderer2 = lr2Obj.AddComponent<LineRenderer>();
                }
                SetupLineRenderer(lineRenderer2);
            
                if (enableVolumetric)
                {
                    _volumetricBeam2 = CreateVolumetricBeam();
                    if (_volumetricBeam2 != null)
                    {
                        _volumetric2Transform = _volumetricBeam2.transform;
                    }
                }
                
                _laserDot2 = Instantiate(laserDotPrefab);
                _laserDot2.SetActive(false);
            }
        
            // Setup shared particle system
            if (enableParticles)
            {
                SetupParticleSystem();
            }
            
            SetLasersActive(false);
        }

        void SetupLineRenderer(LineRenderer lr)
        {
            lr.positionCount = 2;
            lr.startWidth = beamWidth;
            lr.endWidth = beamWidth;
            lr.useWorldSpace = true;
        
            lr.startColor = beamColor;
            lr.endColor = beamColor;
        
            if (beamMaterial != null)
            {
                lr.material = beamMaterial;
            }
            else
            {
                // Try URP shader first, fallback to Unlit/Transparent
                Shader shader = Shader.Find("Universal Render Pipeline/Particles/Unlit");
                if (shader == null)
                {
                    shader = Shader.Find("Unlit/Transparent");
                }
                
                lr.material = new Material(shader);
                lr.material.color = beamColor;
            }
        
            lr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            lr.receiveShadows = false;
            lr.enabled = false;
        }

        GameObject CreateVolumetricBeam()
        {
            GameObject beam;
        
            if (volumetricBeamPrefab != null)
            {
                beam = Instantiate(volumetricBeamPrefab);
            }
            else
            {
                beam = GameObject.CreatePrimitive(PrimitiveType.Cube);
                Destroy(beam.GetComponent<Collider>());
            }
        
            Renderer rendererComp = beam.GetComponent<Renderer>();
        
            if (volumetricMaterial != null)
            {
                rendererComp.material = volumetricMaterial;
            }
            else
            {
                // Try URP shader first, fallback to Unlit/Transparent
                Shader shader = Shader.Find("Universal Render Pipeline/Particles/Unlit");
                if (shader == null)
                {
                    shader = Shader.Find("Unlit/Transparent");
                }
                
                Material volMat = new Material(shader);
                volMat.color = volumetricColor;
                rendererComp.material = volMat;
            }
        
            rendererComp.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            rendererComp.receiveShadows = false;
            beam.SetActive(false);
        
            return beam;
        }

        void SetupParticleSystem()
        {
            if (fogParticles == null)
            {
                GameObject particleObj = new GameObject("LaserFogParticles");
                particleObj.transform.SetParent(transform);
                fogParticles = particleObj.AddComponent<ParticleSystem>();
            }
        
            var main = fogParticles.main;
            main.loop = false;
            main.playOnAwake = false;
            main.maxParticles = 200;
            main.startSpeed = 0f;
            main.startSize = particleSize;
            main.startColor = particleColor;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.startLifetime = particleLifeTime;
        
            var emission = fogParticles.emission;
            emission.enabled = true;
        
            var rendererComp = fogParticles.GetComponent<ParticleSystemRenderer>();
            
            // Try URP shader first, fallback to Unlit/Transparent
            Shader shader = Shader.Find("Universal Render Pipeline/Particles/Unlit");
            if (shader == null)
            {
                shader = Shader.Find("Unlit/Transparent");
            }
            
            rendererComp.material = new Material(shader);
            rendererComp.material.color = particleColor;

            _emitParams = new ParticleSystem.EmitParams();
        }

        void Update()
        {
            if (enableLaser1 && laserOrigin1 != null)
            {
                UpdateLaser(
                    laserOrigin1, 
                    lineRenderer1, 
                    _volumetricBeam1, 
                    _volumetric1Transform,
                    _laserDot1
                );
            }
            else if (enableLaser1)
            {
                HideLaserComponents(lineRenderer1, _volumetricBeam1, _laserDot1);
            }
        
            if (enableLaser2 && laserOrigin2 != null)
            {
                UpdateLaser(
                    laserOrigin2, 
                    lineRenderer2, 
                    _volumetricBeam2, 
                    _volumetric2Transform,
                    _laserDot2
                );
            }
            else if (enableLaser2)
            {
                HideLaserComponents(lineRenderer2, _volumetricBeam2, _laserDot2);
            }
        }

        void UpdateLaser(
            Transform origin, 
            LineRenderer lr, 
            GameObject volBeam,
            Transform volTransform,
            GameObject laserDot
        )
        {
            Vector3 startPos = origin.position;
            RaycastHit hit;
            Vector3 direction = Physics.Raycast(cam.GetRay(), out hit, hitLayers) ? (hit.point - origin.position).normalized : origin.forward;
            
            if (Physics.Raycast(startPos, direction, out hit, laserMaxDistance, hitLayers))
            {
                DrawBeam(startPos, hit.point, lr);

                if (!laserDot.activeSelf)
                {
                    laserDot.SetActive(true);
                }
                
                laserDot.transform.position = hit.point;
                
                if (enableVolumetric && volBeam != null)
                {
                    UpdateVolumetricBeam(startPos, hit.point, direction, volBeam, volTransform);
                }
                
                if (enableParticles && fogParticles != null)
                {
                    EmitFogParticles(startPos, hit.point);
                }
            }
            else
            {
                Vector3 endPos = origin.position + direction * laserMaxDistance;
                DrawBeam(startPos, endPos, lr);
                
                if (laserDot.activeSelf)
                {
                    laserDot.SetActive(false);
                }
                
                if (enableVolumetric && volBeam != null)
                {
                    UpdateVolumetricBeam(startPos, endPos, direction, volBeam, volTransform);
                }
                
                if (enableParticles && fogParticles != null)
                {
                    EmitFogParticles(startPos, endPos);
                }
            }
        }

        void DrawBeam(Vector3 start, Vector3 end, LineRenderer lr)
        {
            if (lr == null)
            {
                return;
            }
            
            if (!lr.enabled)
            {
                lr.enabled = true;
            }
        
            lr.SetPosition(0, start);
            lr.SetPosition(1, end);
        }

        void UpdateVolumetricBeam(Vector3 start, Vector3 end, Vector3 direction, GameObject volBeam, Transform volTransform)
        {
            if (volBeam == null || volTransform == null)
            {
                return;
            }
            
            if (!volBeam.activeSelf)
            {
                volBeam.SetActive(true);
            }
        
            Vector3 midPoint = (start + end) / 2f;
            volTransform.position = midPoint;
            volTransform.rotation = Quaternion.LookRotation(direction);
        
            float distance = Vector3.Distance(start, end);
            volTransform.localScale = new Vector3(volumetricWidth, volumetricWidth, distance);
        }

        void EmitFogParticles(Vector3 start, Vector3 end)
        {
            float distance = Vector3.Distance(start, end);
            int particleCount = Mathf.CeilToInt(distance * particlesPerMeter * Time.deltaTime * 0.5f);
            particleCount = Mathf.Min(particleCount, 3);
        
            for (int i = 0; i < particleCount; i++)
            {
                float t = Random.Range(0f, 1f);
                Vector3 particlePos = Vector3.Lerp(start, end, t);
                particlePos += Random.insideUnitSphere * 0.02f;
            
                _emitParams.position = particlePos;
                fogParticles.Emit(_emitParams, 1);
            }
        }

        void HideLaserComponents(LineRenderer lr, GameObject volBeam, GameObject laserDot)
        {
            if (lr != null && lr.enabled)
            {
                lr.enabled = false;
            }
        
            if (volBeam != null && volBeam.activeSelf)
            {
                volBeam.SetActive(false);
            }

            if (laserDot != null && laserDot.activeSelf)
            {
                laserDot.SetActive(false);
            }
        }

        // Public control methods
        public void SetLasersActive(bool active)
        {
            enableLaser1 = active;
            enableLaser2 = active;
        
            if (!active)
            {
                HideLaserComponents(lineRenderer1, _volumetricBeam1, _laserDot1);
                HideLaserComponents(lineRenderer2, _volumetricBeam2, _laserDot2);
            }
        }

        public void SetLaserColor(Color newColor)
        {
            beamColor = newColor;
        
            if (lineRenderer1 != null)
            {
                lineRenderer1.startColor = newColor;
                lineRenderer1.endColor = newColor;
            }
        
            if (lineRenderer2 != null)
            {
                lineRenderer2.startColor = newColor;
                lineRenderer2.endColor = newColor;
            }
        }

        void OnDestroy()
        {
            if (_volumetricBeam1 != null) Destroy(_volumetricBeam1);
            if (_volumetricBeam2 != null) Destroy(_volumetricBeam2);
        }
    }
}