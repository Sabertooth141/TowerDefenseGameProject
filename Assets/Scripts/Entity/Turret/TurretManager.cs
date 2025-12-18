using System;
using System.Collections.Generic;
using EventSystem;
using UnityEngine;

namespace Entity.Turret
{
    public class TurretManager : MonoBehaviour
    {
        public static TurretManager Instance { get; private set; }
        
        public int maxTurretCount = 10;

        private List<GameObject> _turretObjects = new();

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public void RegisterTurret(GameObject turret)
        {
            if (!_turretObjects.Contains(turret))
            {
                _turretObjects.Add(turret);
                EventHub.TriggerOnTurretUpdate(maxTurretCount - _turretObjects.Count);
            }
        }

        public void UnregisterTurret(GameObject turret)
        {
            _turretObjects.Remove(turret);
            EventHub.TriggerOnTurretUpdate(maxTurretCount - _turretObjects.Count);
        }

        public List<GameObject> GetTurretObjects()
        {
            return _turretObjects;
        }

        public void ClearTurrets()
        {
            _turretObjects.Clear();
            EventHub.TriggerOnTurretUpdate(maxTurretCount - _turretObjects.Count);
        }

        public int GetTurretCount()
        {
            return _turretObjects.Count;
        }
    }
}