using UnityEngine;

namespace Entity.Turret
{
    public class TurretProjectileController : MonoBehaviour
    {
        public float projectileSpeed = 10.0f;
        public float projectileDamage = 10.0f;
        public float projectileLifetime = 10.0f;

        private float _destroyTimer = 0.0f;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
        
        }

        // Update is called once per frame
        void Update()
        {
            _destroyTimer += Time.deltaTime;
            if (_destroyTimer >= projectileLifetime)
            {
                Destroy(gameObject);
            }

            transform.position += transform.forward * (projectileSpeed * Time.deltaTime);
        }

        private void OnTriggerEnter(Collider other)
        {
            // CHANGE LATER TEMPORARY
            if (!other.gameObject.CompareTag("Enemy"))
            {
                return;
            }

            if (!other.gameObject.GetComponent<Entity>())
            {
                Destroy(gameObject);
                return;
            }
            
            other.gameObject.GetComponent<Entity>().TakeDamage(projectileDamage);
            Destroy(gameObject);
        }
    }
}

