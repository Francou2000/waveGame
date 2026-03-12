using System.Collections.Generic;
using UnityEngine;

namespace WaveGame.Combat.Enemy
{
    public sealed class EnemySpawner : MonoBehaviour
    {
        [SerializeField] private EnemyRuntime enemyPrefab;
        [SerializeField] private EnemyDefinitionSO enemyDefinition;
        [SerializeField] private EnemySystem enemySystem;
        [SerializeField] private EnemyDeathSystem deathSystem;
        [SerializeField] private int initialPoolSize = 64;
        [SerializeField] private int maxAlive = 400;
        [SerializeField] private float spawnPerSecond = 8f;
        [SerializeField] private float spawnRadius = 22f;
        [SerializeField, Min(1)] private int maxSpawnsPerFrame = 3;
        [Header("Player Safety")]
        [SerializeField] private Transform playerTransform;
        [SerializeField, Min(0f)] private float minSpawnDistanceFromPlayer = 10f;
        [SerializeField, Min(1)] private int maxSpawnPositionAttempts = 8;

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

            if (playerTransform == null)
            {
                var anchor = FindFirstObjectByType<WaveGame.Combat.Player.PlayerCombatAnchorProvider>();
                if (anchor != null)
                {
                    playerTransform = anchor.CombatAnchor;
                }
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
            var frameSpawnCount = 0;
            var maxSpawnThisFrame = Mathf.Max(1, maxSpawnsPerFrame);
            while (_spawnBudget >= 1f && _alive.Count < allowedAlive && frameSpawnCount < maxSpawnThisFrame)
            {
                _spawnBudget -= 1f;
                SpawnOne();
                frameSpawnCount++;
            }
        }

        private void WarmPool(int amount)
        {
            for (var i = 0; i < amount; i++)
            {
                var enemy = Instantiate(enemyPrefab, transform);
                enemy.gameObject.SetActive(false);
                enemy.SetDefinition(enemyDefinition);
                enemy.Died += OnEnemyDied;
                if (deathSystem != null)
                {
                    deathSystem.Register(enemy);
                }

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
            var spawnPosition = FindSpawnPosition();

            enemy.Activate(spawnPosition);
            _alive.Add(enemy);
            enemySystem.Register(enemy);
        }

        private Vector3 FindSpawnPosition()
        {
            var attempts = Mathf.Max(1, maxSpawnPositionAttempts);
            var fallback = ComputeSpawnPosition();
            if (playerTransform == null || minSpawnDistanceFromPlayer <= 0f)
            {
                return fallback;
            }

            var minDistanceSqr = minSpawnDistanceFromPlayer * minSpawnDistanceFromPlayer;
            for (var i = 0; i < attempts; i++)
            {
                var candidate = ComputeSpawnPosition();
                if ((candidate - playerTransform.position).sqrMagnitude >= minDistanceSqr)
                {
                    return candidate;
                }
            }

            return fallback;
        }

        private Vector3 ComputeSpawnPosition()
        {
            var ringDirection = Random.insideUnitCircle;
            if (ringDirection.sqrMagnitude <= 0.0001f)
            {
                ringDirection = Vector2.right;
            }

            ringDirection.Normalize();
            var spawnPosition = transform.position + new Vector3(ringDirection.x, 0f, ringDirection.y) * spawnRadius;
            spawnPosition.y = transform.position.y;
            return spawnPosition;
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

        private void OnDestroy()
        {
            if (deathSystem == null)
            {
                return;
            }

            foreach (var enemy in _pool)
            {
                deathSystem.Unregister(enemy);
            }

            foreach (var enemy in _alive)
            {
                deathSystem.Unregister(enemy);
            }
        }
    }
}
