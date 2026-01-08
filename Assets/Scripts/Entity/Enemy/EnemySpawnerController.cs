using System;
using System.Collections;
using System.Collections.Generic;
using GameEvents;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

namespace Entity.Enemy
{
    public class EnemySpawnerController : MonoBehaviour
    {
        [Header("Enemy Prefabs")]
        public GameObject[] enemyPrefabs;

        [Header("Spawner Settings")]
        public float spawnRadius;
        public int maxEnemies;
        public float checkInterval;

        private List<Transform> _activeSpawnPos = new();
        private bool _gameRunning;

        private void Awake()
        {
            EventHub.OnGameStart += OnGameStart;
            EventHub.OnGameEnd += OnGameEnd;
            EventHub.OnGeneratorStart += OnGeneratorStart;
        }

        private void OnDestroy()
        {
            EventHub.OnGameStart -= OnGameStart;
            EventHub.OnGameEnd -= OnGameEnd;
            EventHub.OnGeneratorStart -= OnGeneratorStart;
        }

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            InitSpawnPoints();
        }

        private void OnGeneratorStart()
        {
            StartCoroutine(SpawnLoop());
        }

        private void OnGameEnd()
        {
            _gameRunning = false;
        }

        private void OnGameStart()
        {
            _gameRunning = true;
        }

        private void InitSpawnPoints()
        {
            GameObject[] activeSpawnPoints = GameObject.FindGameObjectsWithTag("EnemySpawnPoint");
            foreach (GameObject spawnPoint in activeSpawnPoints)
            {
                _activeSpawnPos.Add(spawnPoint.transform);
            }
        }

        //TODO: WIP
        private IEnumerator SpawnLoop()
        {
            while (_gameRunning)
            {
                int currentEnemies = EnemyManager.Instance.GetEnemyCount();

                if (currentEnemies < maxEnemies)
                {
                    for (int i = 0; i < maxEnemies; i++)
                    {
                        SpawnEnemy();
                    }
                }

                yield return new WaitForSeconds(checkInterval);
            }
        }

        private void SpawnEnemy()
        {
            if (_activeSpawnPos.Count == 0)
            {
                return;
            }

            Transform spawnPoint = _activeSpawnPos[Random.Range(0, _activeSpawnPos.Count)];

            if (GetRandomPointOnNavMesh(spawnPoint.position, spawnRadius, out Vector3 spawnPosition))
            {
                Instantiate(enemyPrefabs[Random.Range(0, enemyPrefabs.Length)], spawnPosition, spawnPoint.rotation);
            }
            else
            {
                Instantiate(enemyPrefabs[Random.Range(0, enemyPrefabs.Length)],
                    spawnPoint.position,
                    spawnPoint.rotation);
            }
        }

        public void SpawnEnemies(int count)
        {
            return;
        }

        public bool GetRandomPointOnNavMesh(Vector3 center, float range, out Vector3 result)
        {
            Vector3 randomPoint = center + Random.insideUnitSphere * range;

            NavMeshHit meshHit;
            if (NavMesh.SamplePosition(randomPoint, out meshHit, 1.0f, NavMesh.AllAreas))
            {
                result = meshHit.position;
                return true;
            }

            result = Vector3.zero;
            return false;
        }
    }
}