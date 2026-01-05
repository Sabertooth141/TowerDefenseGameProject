using System;
using UnityEngine;

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
        public static event Action OnGeneratorStart;
        
        // enemy goal hurt
        public static event Action<float, float> OnGoalHurt;
        
        // building system events
        public static event Action OnBuildingPressed;
        public static event Action OnBuildingConfirmed;
        public static event Action<int> OnTurretUpdate;
        
        // terminal events
        public static event Action<bool, Transform> OnTerminalStatusChanged;
        public static event Action OnExecuteCommand;
        public static event Action OnCommandComplete;
        public static event Action OnUploadFileComplete;
        
        // internal logic events
        public static event Action OnFilesGenerated;
        
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
        
        public static void TriggerOnGoalHurt(float curHp, float maxHp)
        {
            OnGoalHurt?.Invoke(curHp, maxHp);
        }

        public static void TriggerOnTurretUpdate(int availableTurret)
        {
            OnTurretUpdate?.Invoke(availableTurret);
        }

        public static void TriggerOnFilesGenerated()
        {
            OnFilesGenerated?.Invoke();
        }

        public static void TriggerOnTerminalStatusChanged(bool terminalStatus, Transform terminalScreen)
        {
            OnTerminalStatusChanged?.Invoke(terminalStatus, terminalScreen);
        }

        public static void TriggerOnExecuteCommand()
        {
            OnExecuteCommand?.Invoke();
        }

        public static void TriggerOnCommandCompleted()
        {
            OnCommandComplete?.Invoke();
        }

        public static void TriggerOnUploadFileComplete()
        {
            OnUploadFileComplete?.Invoke();
        }
        
        public static void TriggerOnGeneratorStart()
        {
            OnGeneratorStart?.Invoke();
        }
    }
}