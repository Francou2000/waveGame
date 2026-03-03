using System.Collections.Generic;
using UnityEngine;

namespace WaveGame.Combat.Enemy
{
    public sealed class EnemySpawner : MonoBehaviour
    {
        [SerializeField] private EnemyRuntime enemyPrefab;
        [SerializeField] private EnemySystem enemySystem;
        [SerializeField] private int initialPoolSize = 64;
        [SerializeField] private int maxAlive = 400;
        [SerializeField] private float spawnPerSecond = 20f;
        [SerializeField] private float spawnRadius = 22f;

        private readonly Queue<EnemyRuntime> _pool = new();
        private readonly HashSet<EnemyRuntime> _alive = new();
        private float _spawnBudget;


        public void ConfigureRuntime(int newMaxAlive, float newSpawnPerSecond, float newSpawnRadius)
        {
            maxAlive = Mathf.Max(1, newMaxAlive);
            spawnPerSecond = Mathf.Max(0f, newSpawnPerSecond);
            spawnRadius = Mathf.Max(1f, newSpawnRadius);
        }

        private void Awake()
        {
            if (enemyPrefab == null)
            {
                return;
            }

            WarmPool(Mathf.Max(1, initialPoolSize));
        }

        private void Update()
        {
            if (enemyPrefab == null || enemySystem == null)
            {
                return;
            }

            _spawnBudget += spawnPerSecond * Time.deltaTime;
            var allowedAlive = Mathf.Max(1, maxAlive);
            while (_spawnBudget >= 1f && _alive.Count < allowedAlive)
            {
                _spawnBudget -= 1f;
                SpawnOne();
            }
        }

        private void WarmPool(int amount)
        {
            for (var i = 0; i < amount; i++)
            {
                var enemy = Instantiate(enemyPrefab, transform);
                enemy.gameObject.SetActive(false);
                enemy.Died += OnEnemyDied;
                _pool.Enqueue(enemy);
            }
        }

        private void SpawnOne()
        {
            if (_pool.Count == 0)
            {
                WarmPool(16);
            }

            var enemy = _pool.Dequeue();
            var spawnPosition = transform.position + Random.onUnitSphere * spawnRadius;
            spawnPosition.y = transform.position.y;

            enemy.Activate(spawnPosition);
            _alive.Add(enemy);
            enemySystem.Register(enemy);
        }

        private void OnEnemyDied(EnemyRuntime enemy)
        {
            if (enemy == null)
            {
                return;
            }

            _alive.Remove(enemy);
            enemySystem.Unregister(enemy);
            _pool.Enqueue(enemy);
        }
    }
}
