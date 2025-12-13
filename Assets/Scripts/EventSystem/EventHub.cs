using System;
using Entity;

namespace EventSystem
{
    public class EventHub
    {
        // event declaration
        // enemy events
        public static event Action<Entity.Entity> OnEnemyDied;
        
        // game events
        public static event Action OnGameStart;
        public static event Action OnGameEnd;
        
        // building system events
        public static event Action OnBuildingPressed;
        public static event Action OnBuildingConfirmed;
        
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

        public static void TriggerBuildingPressed()
        {
            OnBuildingPressed?.Invoke();
        }

        public static void TriggerBuildingConfirmed()
        {
            OnBuildingConfirmed?.Invoke();
        }
    }
}