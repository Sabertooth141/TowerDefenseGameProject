using System;
using EventSystem;
using UnityEngine;

namespace Entity.Enemy
{
    public class EnemyController : Entity
    {

        protected override void Start()
        {
            base.Start();
            EnemyManager.Instance.RegisterEnemy(this);
        }

        // Update is called once per frame
        protected override void Update()
        {
            base.Update();
        }

        public override void TakeDamage(float damage)
        {
            base.TakeDamage(damage);
        }

        protected override void Die()
        {
            EventHub.TriggerEnemyDied(this);
            base.Die();
        }

        private void OnDestroy()
        {
            EnemyManager.Instance?.UnregisterEnemy(this);
        }
    }
}