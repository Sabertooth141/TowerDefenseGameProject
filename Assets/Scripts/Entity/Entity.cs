using System;
using EventSystem;
using UnityEngine;

namespace Entity
{
    public class Entity : MonoBehaviour
    {
        public float maxHp = 100.0f;
        public float currHp;

        protected virtual void Start()
        {
            currHp = maxHp;
        }

        public virtual void TakeDamage(float damage)
        {
            Debug.Log("HURTING");
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

        public virtual void Die()
        {
            Destroy(gameObject);
        }
        
    }
}