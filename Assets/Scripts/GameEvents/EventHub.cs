using System;
using UnityEngine;

namespace GameEvents
{
    public class EventHub
    {
        // event declaration
        // enemy events
        public static event Action<Entity.Entity> OnEnemyDied;
        
        // game events
        public static event Action OnGameStart;
        public static event Action OnGameEnd;
        public static event Action OnStartScene;
        public static event Action OnPlayerControl;
        public static event Action OnVictory;
        
        //generator events
        public static event Action OnGeneratorStarting;
        public static event Action OnGeneratorStart;
        public static event Action OnTryTurnOffGenerator;
        public static event Action OnStopTurnOffGenerator;
        public static event Action OnGeneratorTurnOff;
        
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
        public static event Action OnTerminalsRegistered;
        
        // player events
        public static event Action<float> OnPlayerHurt;
        
        // SFX events
        public static event Action OnBGMStart;
        public static event Action OnBGMEnd;
        public static event Action OnAlarmStart;
        public static event Action OnAlarmEnd;
        public static event Action OnMusicAllStop;
        
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

        public static void TriggerOnGeneratorStarting()
        {
            OnGeneratorStarting?.Invoke();
        }
        
        public static void TriggerOnGeneratorStart()
        {
            OnGeneratorStart?.Invoke();
        }
        
        public static void TriggerOnTryTurnOffGenerator()
        {
            OnTryTurnOffGenerator?.Invoke();
        }

        public static void TriggerOnStopTurnOffGenerator()
        {
            OnStopTurnOffGenerator?.Invoke();
        }

        public static void TriggerOnGeneratorTurnOff()
        {
            OnGeneratorTurnOff?.Invoke();
        }

        public static void TriggerOnStartScene()
        {
            OnStartScene?.Invoke();
        }

        public static void TriggerOnPlayerControl()
        {
            OnPlayerControl?.Invoke();
        }

        public static void TriggerOnPlayerHurt(float playerHp)
        {
            OnPlayerHurt?.Invoke(playerHp);
        }

        public static void TriggerOnTerminalRegistered()
        {
            OnTerminalsRegistered?.Invoke();
        }

        public static void TriggerOnBGMStart()
        {
            OnBGMStart?.Invoke();
        }

        public static void TriggerOnBGMEnd()
        {
            OnBGMEnd?.Invoke();
        }

        public static void TriggerOnAlarmStart()
        {
            OnAlarmStart?.Invoke();
        }

        public static void TriggerOnAlarmEnd()
        {
            OnAlarmEnd?.Invoke();
        }

        public static void TriggerOnMusicAllStop()
        {
            OnMusicAllStop?.Invoke();
        }

        public static void TriggerOnVictory()
        {
            OnVictory?.Invoke();
        }
    }
}