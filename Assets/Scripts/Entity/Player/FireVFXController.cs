using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Entity.Player
{
    [System.Serializable]
    public class FiringPoint
    {
        public Transform firingPoint;
        public ParticleSystem muzzleFlash;
        public ParticleSystem smoke;
        public GameObject muzzleFlashLight;
        public Transform tracerFiringPoint;
    }

    public class FireVFXController : MonoBehaviour
    {
        [Header("Firing Points")]
        [SerializeField] private FiringPoint[] firingPoints;

        [Header("Tracer")]
        [SerializeField] private ParticleSystem tracer;
        [SerializeField] private int tracerChance = 5;

        [Header("Impact")]
        [SerializeField] private ParticleSystem hitImpact;

        private int _firingPointIndex = 0;
        private FiringPoint CurrentFiringPoint
        {
            get { return firingPoints[_firingPointIndex]; }
        }

        public void PlayShotEffects(Vector3 fireDirection, Vector3 hitPoint)
        {
            CurrentFiringPoint.muzzleFlash.Play();
            CurrentFiringPoint.smoke.Play();

            StartCoroutine(PlayMuzzleFlashLight());

            if (Random.Range(0, tracerChance) == 0)
            {
                var tracerMain = tracer.main;

                float speed = tracerMain.startSpeed.constant;
                float hitDistance = Vector3.Distance(CurrentFiringPoint.tracerFiringPoint.position, hitPoint);
                tracerMain.startLifetime = hitDistance / speed;

                tracer.transform.position = CurrentFiringPoint.tracerFiringPoint.position;
                tracer.transform.rotation = Quaternion.LookRotation(fireDirection);
                tracer.Play();
            }

            _firingPointIndex = (_firingPointIndex + 1) % firingPoints.Length;
        }

        public void PlayShotEffects(Vector3 fireDirection)
        {
            CurrentFiringPoint.muzzleFlash.Play();
            CurrentFiringPoint.smoke.Play();

            StartCoroutine(PlayMuzzleFlashLight());

            if (Random.Range(0, tracerChance) == 0)
            {
                tracer.transform.position = CurrentFiringPoint.tracerFiringPoint.position;
                tracer.transform.rotation = Quaternion.LookRotation(fireDirection);
                tracer.Play();
            }

            _firingPointIndex = (_firingPointIndex + 1) % firingPoints.Length;
        }

        private IEnumerator PlayMuzzleFlashLight()
        {
            if (CurrentFiringPoint == null)
            {
                yield break;
            }
            
            GameObject currLight = CurrentFiringPoint.muzzleFlashLight;
            
            currLight.SetActive(true);

            yield return null;
            yield return null;

            currLight.SetActive(false);
        }

        public void PlayHitEffect(RaycastHit hit)
        {
            hitImpact.transform.position = hit.point;
            hitImpact.transform.rotation = Quaternion.LookRotation(hit.normal);
            hitImpact.Play();
        }
    }
}