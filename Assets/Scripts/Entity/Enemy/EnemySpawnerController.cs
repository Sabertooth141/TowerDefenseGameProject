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

        private Coroutine _spawnCoroutine;

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

        void Start()
        {
            InitSpawnPoints();
        }

        private void OnGameStart()
        {
            _gameRunning = true;
        }

        private void OnGameEnd()
        {
            _gameRunning = false;

            if (_spawnCoroutine != null)
            {
                StopCoroutine(_spawnCoroutine);
                _spawnCoroutine = null;
            }
        }

        private void OnGeneratorStart()
        {
            if (_spawnCoroutine == null)
            {
                _spawnCoroutine = StartCoroutine(SpawnLoop());
            }
        }

        private void InitSpawnPoints()
        {
            GameObject[] activeSpawnPoints = GameObject.FindGameObjectsWithTag("EnemySpawnPoint");
            foreach (GameObject spawnPoint in activeSpawnPoints)
            {
                _activeSpawnPos.Add(spawnPoint.transform);
            }
        }

        private IEnumerator SpawnLoop()
        {
            while (_gameRunning)
            {
                int currentEnemies = EnemyManager.Instance.GetEnemyCount();

                if (currentEnemies < maxEnemies)
                {
                    int spawnCount = Mathf.Clamp(
                        Random.Range(
                            (maxEnemies - currentEnemies) / 2,
                            maxEnemies - currentEnemies
                        ),
                        1,
                        maxEnemies // hard cap per interval
                    );

                    for (int i = 0; i < spawnCount; i++)
                    {
                        SpawnEnemy();
                    }
                }

                yield return new WaitForSeconds(checkInterval);
            }

            _spawnCoroutine = null;
        }

        private void SpawnEnemy()
        {
            if (_activeSpawnPos.Count == 0 || enemyPrefabs.Length == 0)
            {
                return;
            }

            Transform spawnPoint = _activeSpawnPos[Random.Range(0, _activeSpawnPos.Count)];

            if (GetRandomPointOnNavMesh(spawnPoint.position, spawnRadius, out Vector3 spawnPosition))
            {
                Instantiate(
                    enemyPrefabs[Random.Range(0, enemyPrefabs.Length)],
                    spawnPosition,
                    spawnPoint.rotation
                );
            }
            else
            {
                Instantiate(
                    enemyPrefabs[Random.Range(0, enemyPrefabs.Length)],
                    spawnPoint.position,
                    spawnPoint.rotation
                );
            }
        }

        public bool GetRandomPointOnNavMesh(Vector3 center, float range, out Vector3 result)
        {
            Vector3 randomPoint = center + Random.insideUnitSphere * range;

            if (NavMesh.SamplePosition(randomPoint, out NavMeshHit hit, 1.0f, NavMesh.AllAreas))
            {
                result = hit.position;
                return true;
            }

            result = Vector3.zero;
            return false;
        }
    }
}
