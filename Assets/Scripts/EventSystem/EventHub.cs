using System;
using Entity;

namespace EventSystem
{
    public class EventHub
    {
        // event declaration
        public static event Action<Entity.Entity> OnEnemyDied;
        public static event Action OnGameStart;
        public static event Action OnGameEnd;
        
        // event trigger
        public static void TriggerEnemyDied(Entity.Entity enemy)
        {
            OnEnemyDied?.Invoke(enemy);
        }

        public static void TriggerGameStart()
        {
            OnGameStart?.Invoke();
        }

        public static void TriggerGameEnd()
        {
            OnGameEnd?.Invoke();
        }
    }
}