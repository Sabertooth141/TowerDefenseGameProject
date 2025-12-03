using System;
using Entity;

namespace EventSystem
{
    public class EventHub
    {
        // event declaration
        public static event Action<Entity.Entity> OnEnemyDied;
        
        // event trigger
        public static void TriggerEnemyDied(Entity.Entity enemy)
        {
            OnEnemyDied?.Invoke(enemy);
        }
    }
}