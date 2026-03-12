using UnityEngine;

namespace WaveGame.Combat.Projectiles
{
    /// <summary>
    /// Runtime helper to stress projectile spawning throughput.
    /// Attach to an empty GameObject, assign system + definition, and press Play.
    /// </summary>
    public sealed class ProjectileStressTester : MonoBehaviour
    {
        [SerializeField] private ProjectileSystem projectileSystem;
        [SerializeField] private ProjectileDefinition projectileDefinition;
        [SerializeField] private int ownerEntityId = 999;
        [SerializeField] private int teamId = 1;

        [Header("Spawn")]
        [SerializeField, Min(1f)] private float projectilesPerSecond = 1000f;
        [SerializeField] private bool randomPlanarDirection = true;
        [SerializeField] private bool useTransformForwardWhenNotRandom = true;

        [Header("Logging")]
        [SerializeField, Min(0.1f)] private float logInterval = 1f;

        private float _spawnBudget;
        private float _nextLogTime;
        private int _spawnedSinceLastLog;
        private int _failedSinceLastLog;

        private void Awake()
        {
            if (projectileSystem == null)
            {
                projectileSystem = FindFirstObjectByType<ProjectileSystem>();
            }
        }

        private void Update()
        {
            if (projectileSystem == null || projectileDefinition == null)
            {
                return;
            }

            _spawnBudget += projectilesPerSecond * Time.deltaTime;
            while (_spawnBudget >= 1f)
            {
                _spawnBudget -= 1f;
                TrySpawnOne();
            }

            if (Time.time >= _nextLogTime)
            {
                _nextLogTime = Time.time + logInterval;
                Debug.Log($"[ProjectileStressTester] spawned={_spawnedSinceLastLog} failed={_failedSinceLastLog} active={projectileSystem.ActiveProjectileCount} peak={projectileSystem.PeakActiveProjectiles} queries={projectileSystem.PhysicsQueriesThisFrame}");
                _spawnedSinceLastLog = 0;
                _failedSinceLastLog = 0;
            }
        }

        private void TrySpawnOne()
        {
            var direction = ResolveDirection();
            var context = new ProjectileSpawnContext(projectileDefinition, ownerEntityId, teamId, transform.position, direction);
            if (projectileSystem.TrySpawn(context))
            {
                _spawnedSinceLastLog++;
            }
            else
            {
                _failedSinceLastLog++;
            }
        }

        private Vector3 ResolveDirection()
        {
            if (randomPlanarDirection)
            {
                var angle = Random.value * 360f;
                var dir = Quaternion.Euler(0f, angle, 0f) * Vector3.forward;
                return dir.normalized;
            }

            var forward = useTransformForwardWhenNotRandom ? transform.forward : Vector3.forward;
            forward.y = 0f;
            return forward.sqrMagnitude > 0.0001f ? forward.normalized : Vector3.forward;
        }
    }
}
