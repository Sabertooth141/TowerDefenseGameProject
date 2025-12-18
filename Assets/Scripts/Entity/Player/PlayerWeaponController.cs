using System;
using Entity.Player;
using UnityEngine;

namespace Entity.Player
{
    public class PlayerWeaponController : MonoBehaviour
    {
        [Header("References")]
        public PlayerController playerController;
        public CameraController cameraController;
        [SerializeField] private PlayerInputReader inputReader;
        public GameObject hitIndicate;

        [Header("Weapon Settings")]
        public float fireRate = 0.1f;
        public float damage = 20f;
        public float range = 200f;

        private float _nextFireTime = 0;

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
            if (cameraController.IsAiming() && inputReader.ShootPressed && Time.time >= _nextFireTime)
            {
                Shoot();
                _nextFireTime = Time.time + fireRate;
            }
        }

        private void Shoot()
        {
            if (Physics.Raycast(cameraController.GetRay(), out RaycastHit hit, range))
            {
                Instantiate(hitIndicate, hit.point, hit.transform.rotation);
                if (hit.transform.CompareTag("Enemy"))
                {
                    hit.transform.gameObject.GetComponent<Entity>().TakeDamage(damage);
                }
            }
        }

    }
}