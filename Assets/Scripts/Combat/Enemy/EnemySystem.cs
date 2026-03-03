using System.Collections.Generic;
using UnityEngine;

namespace WaveGame.Combat.Enemy
{
    public sealed class EnemySystem : MonoBehaviour
    {
        [SerializeField] private Transform playerTarget;
        [SerializeField] private LayerMask enemyLayerMask;
        [SerializeField] private float seekWeight = 1f;
        [SerializeField] private float separationWeight = 1.5f;
        [SerializeField] private float separationRadius = 1f;
        [SerializeField] private float separationTickInterval = 0.08f;

        private readonly List<EnemyRuntime> _activeEnemies = new(1024);
        private readonly Dictionary<int, float> _nextSeparationByEnemy = new(1024);
        private readonly Dictionary<int, Vector3> _separationCacheByEnemy = new(1024);
        private readonly Collider[] _separationHits = new Collider[64];

        public void Register(EnemyRuntime enemy)
        {
            if (enemy == null || _activeEnemies.Contains(enemy))
            {
                return;
            }

            _activeEnemies.Add(enemy);
            _nextSeparationByEnemy[enemy.EntityId] = 0f;
            _separationCacheByEnemy[enemy.EntityId] = Vector3.zero;
        }

        public void Unregister(EnemyRuntime enemy)
        {
            if (enemy == null)
            {
                return;
            }

            _activeEnemies.Remove(enemy);
            _nextSeparationByEnemy.Remove(enemy.EntityId);
            _separationCacheByEnemy.Remove(enemy.EntityId);
        }

        private void Update()
        {
            if (playerTarget == null)
            {
                return;
            }

            var dt = Time.deltaTime;
            var now = Time.time;

            for (var i = _activeEnemies.Count - 1; i >= 0; i--)
            {
                var enemy = _activeEnemies[i];
                if (enemy == null || !enemy.IsAlive)
                {
                    _activeEnemies.RemoveAt(i);
                    continue;
                }

                var seekDirection = (playerTarget.position - enemy.transform.position);
                seekDirection.y = 0f;
                if (seekDirection.sqrMagnitude > 0.0001f)
                {
                    seekDirection.Normalize();
                }

                var enemyId = enemy.EntityId;
                if (!_nextSeparationByEnemy.TryGetValue(enemyId, out var nextTick) || now >= nextTick)
                {
                    _separationCacheByEnemy[enemyId] = ComputeSeparation(enemy);
                    _nextSeparationByEnemy[enemyId] = now + Mathf.Max(0.02f, separationTickInterval);
                }

                var separation = _separationCacheByEnemy[enemyId];
                var finalDir = (seekDirection * seekWeight) + (separation * separationWeight);
                finalDir.y = 0f;
                if (finalDir.sqrMagnitude > 0.0001f)
                {
                    finalDir.Normalize();
                    enemy.transform.position += finalDir * enemy.MoveSpeed * dt;
                    enemy.transform.rotation = Quaternion.LookRotation(finalDir, Vector3.up);
                }
            }
        }

        private Vector3 ComputeSeparation(EnemyRuntime enemy)
        {
            var center = enemy.transform.position;
            var hitCount = Physics.OverlapSphereNonAlloc(center, separationRadius, _separationHits, enemyLayerMask, QueryTriggerInteraction.Ignore);
            var push = Vector3.zero;

            for (var i = 0; i < hitCount; i++)
            {
                var col = _separationHits[i];
                if (col == null)
                {
                    continue;
                }

                if (!col.TryGetComponent<EnemyRuntime>(out var other) || other == enemy || !other.IsAlive)
                {
                    continue;
                }

                var away = center - other.transform.position;
                away.y = 0f;
                var sqrDistance = Mathf.Max(away.sqrMagnitude, 0.0001f);
                push += away.normalized / sqrDistance;
            }

            if (push.sqrMagnitude > 0.0001f)
            {
                push.Normalize();
            }

            return push;
        }
    }
}
