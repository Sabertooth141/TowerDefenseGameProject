using EventSystem;
using UnityEngine;

namespace Entity
{
    public class EnemyGoalController : Entity
    {
        protected override void Start()
        {
            base.Start();
        }

        protected override void Update()
        {
            base.Update();
        }

        public override void TakeDamage(float damage)
        {
            base.TakeDamage(damage);
            Debug.Log("Core taking damage: " + damage);
        }

        protected override void Die()
        {
            Debug.Log("Core destroyed, lost");
            EventHub.TriggerGameEnd();
            Destroy(gameObject);
        }
    }
}