using System;
using System.Collections.Generic;
using UnityEngine;

namespace Entity.Turret
{
    public class TurretManager : MonoBehaviour
    {
        public static TurretManager Instance { get; private set; }
        
        private List<GameObject> _turretObjects;

        private void Awake()
        {
            _turretObjects = new List<GameObject>();
            
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
                _turretObjects.Add(turret);
        }
        
        public void UnregisterTurret(GameObject turret)
        {
            _turretObjects.Remove(turret);
        }

        public List<GameObject> GetTurretObjects()
        {
            return  _turretObjects;
        }

        public void ClearTurrets()
        {
            _turretObjects.Clear();
        }

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
        }

        // Update is called once per frame
        void Update()
        {
        }
    }
}