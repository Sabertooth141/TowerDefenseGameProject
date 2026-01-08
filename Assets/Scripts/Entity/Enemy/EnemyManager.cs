using System.Collections.Generic;
using Entity.Enemy;
using UnityEngine;

namespace Entity.Enemy
{
    public class EnemyManager : MonoBehaviour
    {
        public static EnemyManager Instance { get; private set; }

        private List<GameObject> _activeEnemies;

        void Awake()
        {
            _activeEnemies = new List<GameObject>();

            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public void RegisterEnemy(GameObject enemy)
        {
            if (!_activeEnemies.Contains(enemy))
            {
                _activeEnemies.Add(enemy);
            }
        }

        public void UnregisterEnemy(GameObject enemy)
        {
            _activeEnemies.Remove(enemy);
        }

        public List<GameObject> GetActiveEnemies()
        {
            return _activeEnemies;
        }

        public int GetEnemyCount()
        {
            return _activeEnemies.Count;
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