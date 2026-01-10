using System;
using System.Collections;
using System.Collections.Generic;
using Entity.Player;
using UnityEngine;
using UnityEngine.Serialization;
using Random = System.Random;

namespace Entity.Player
{
    public class PlayerWeaponController : MonoBehaviour
    {
        [Header("References")]
        public PlayerController playerController;
        public Transform playerModelTransform;
        public CameraController cameraController;
        public GameObject hitIndicate;
        public LayerMask hitLayer;
        [SerializeField] private PlayerInputReader inputReader;
        [SerializeField] private RecoilController recoilController;
        [SerializeField] private FireVFXController fireVFXController;
        [SerializeField] private WeaponSFXController sfxController;
        
        [Header("Weapon Settings")]
        public float fireRate = 0.1f;
        public float damage = 20f;
        public float range = 200f;

        private float _nextFireTime = 0;
        private bool _wasFiringLastFrame = false;

        private void Awake()
        {
            if (inputReader == null)
            {
                Debug.LogError("PlayerWeaponController: input reader not found");
            }
        }

        // Update is called once per frame
        void Update()
        {
            bool isFiring = cameraController.IsAiming() && inputReader.ShootPressed;
            if (isFiring && Time.time >= _nextFireTime)
            {
                Shoot();
                _nextFireTime = Time.time + fireRate;
            }

            if (_wasFiringLastFrame && !isFiring)
            {
                recoilController.ResetRecoil();
            }
            
            _wasFiringLastFrame = isFiring;
        }

        private void Shoot()
        {
            recoilController.ApplyRecoil();
            fireVFXController.PlayShotEffects(cameraController.GetRay().direction);
            sfxController.StartFiring();
            if (Physics.Raycast(cameraController.GetRay(), out RaycastHit hit, range, hitLayer))
            {
                fireVFXController.PlayHitEffect(hit);
                Instantiate(hitIndicate, hit.point, hit.transform.rotation);
                
                if (hit.transform.CompareTag("Enemy"))
                {
                    Entity entity = hit.transform.gameObject.GetComponent<Entity>();
                    if (entity == null)
                    {
                        entity = hit.transform.gameObject.GetComponentInParent<Entity>();
                    }
                    entity.TakeDamage(damage);
                }
            }
        }
    }
}