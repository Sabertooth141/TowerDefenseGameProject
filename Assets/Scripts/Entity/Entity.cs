using System;
using UnityEngine;

namespace Entity
{
    public class Entity : MonoBehaviour
    {
        [Header("Entity Settings")]
        public float maxHp = 100.0f;
        public float currHp;

        protected virtual void Start()
        {
            currHp = maxHp;
        }

        protected virtual void Update()
        {
        }

        public virtual void TakeDamage(float damage)
        {
            if (damage <= 0.0)
            {
                return;
            }

            currHp -= damage;
            if (currHp > 0)
            {
                return;
            }

            currHp = 0;
            Die();
        }

        protected virtual void Die()
        {
            if (gameObject == null)
            {
                return;
            }
            
            Destroy(gameObject);
        }

    }
}