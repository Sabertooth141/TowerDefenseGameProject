using EventSystem;
using UnityEngine;

namespace Entity
{
    public class TestTargetController : Entity
    {
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        protected override void Start()
        {
            base.Start();
        }

        // Update is called once per frame
        void Update()
        {
        }

        public override void TakeDamage(float damage)
        {
            base.TakeDamage(damage);
        }

        public override void Die()
        {
            EventHub.TriggerEnemyDied(this);
            base.Die();
        }
    }
}