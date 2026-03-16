using UnityEngine;
using WaveGame.Combat.Projectiles;

namespace WaveGame.Combat.Boss
{
    public sealed class SimpleBossAttackController : MonoBehaviour
    {
        [SerializeField] private ProjectileSystem projectileSystem;
        [SerializeField] private ProjectileDefinition projectileDefinition;
        [SerializeField] private Transform firePoint;
        [SerializeField] private Transform playerTarget;
        [SerializeField, Min(0.1f)] private float shotCooldown = 1.2f;
        [SerializeField, Min(0.1f)] private float radialCooldown = 4.5f;
        [SerializeField, Min(3)] private int radialCount = 10;
        [SerializeField] private int ownerEntityId = 20000;
        [SerializeField] private int teamId = 2;

        private float _nextShot;
        private float _nextRadial;

        private void Awake()
        {
            if (projectileSystem == null)
            {
                projectileSystem = FindFirstObjectByType<ProjectileSystem>();
            }

            if (playerTarget == null)
            {
                var player = FindFirstObjectByType<WaveGame.Combat.Player.PlayerCombatAnchorProvider>();
                if (player != null)
                {
                    playerTarget = player.transform;
                }
            }
        }

        private void Update()
        {
            if (projectileSystem == null || projectileDefinition == null)
            {
                return;
            }

            var now = Time.time;
            if (now >= _nextShot)
            {
                _nextShot = now + shotCooldown;
                FireAtPlayer();
            }

            if (now >= _nextRadial)
            {
                _nextRadial = now + radialCooldown;
                FireRadial();
            }
        }

        private void FireAtPlayer()
        {
            if (playerTarget == null)
            {
                return;
            }

            var origin = firePoint != null ? firePoint.position : transform.position + Vector3.up;
            var toPlayer = playerTarget.position - origin;
            toPlayer.y = 0f;
            if (toPlayer.sqrMagnitude <= 0.0001f)
            {
                return;
            }

            projectileSystem.TrySpawn(new ProjectileSpawnContext(projectileDefinition, ownerEntityId, teamId, origin, toPlayer.normalized));
        }

        private void FireRadial()
        {
            var origin = firePoint != null ? firePoint.position : transform.position + Vector3.up;
            for (var i = 0; i < radialCount; i++)
            {
                var t = i / (float)radialCount;
                var angle = t * 360f;
                var direction = Quaternion.Euler(0f, angle, 0f) * Vector3.forward;
                projectileSystem.TrySpawn(new ProjectileSpawnContext(projectileDefinition, ownerEntityId, teamId, origin, direction));
            }
        }
    }
}
